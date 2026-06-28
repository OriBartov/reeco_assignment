using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderOps.Infrastructure.Bulk;
using OrderOps.Infrastructure.Caching;
using OrderOps.Infrastructure.Common;
using OrderOps.Infrastructure.Data;

namespace OrderOps.Infrastructure.Workers;

public sealed class BulkJobBackgroundService(
    IBulkJobQueue queue,
    IBulkOrderLockService orderLockService,
    IServiceScopeFactory scopeFactory,
    ILogger<BulkJobBackgroundService> logger) : BackgroundService
{
    private const int BatchSize = 100;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessJobAsync(message, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Bulk job {JobId} failed unexpectedly", message.JobId);
                await MarkJobFailedAsync(message.JobId, stoppingToken);
            }
        }
    }

    private async Task ProcessJobAsync(BulkJobMessage message, CancellationToken cancellationToken)
    {
        var completed = 0;
        var failed = 0;
        var statusChanged = false;

        for (var offset = 0; offset < message.OrderIds.Count; offset += BatchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var batch = message.OrderIds.Skip(offset).Take(BatchSize).ToList();

            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var (batchCompleted, batchFailed) = await ProcessBatchAsync(
                db,
                orderLockService,
                batch,
                message.Action,
                message.Reason,
                cancellationToken);

            completed += batchCompleted;
            failed += batchFailed;

            if (batchCompleted > 0 &&
                (message.Action is BulkActions.Approve or BulkActions.Reject))
            {
                statusChanged = true;
            }

            await UpdateJobProgressAsync(db, message.JobId, completed, failed, cancellationToken);
        }

        if (statusChanged)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var statsCache = scope.ServiceProvider.GetRequiredService<IStatsCache>();
            await statsCache.InvalidateStatsAsync(cancellationToken);
        }

        await FinalizeJobAsync(message.JobId, completed, failed, cancellationToken);

        logger.LogInformation(
            "Bulk job {JobId} finished: action={Action}, completed={Completed}, failed={Failed}",
            message.JobId,
            message.Action,
            completed,
            failed);
    }

    private static async Task<(int Completed, int Failed)> ProcessBatchAsync(
        AppDbContext db,
        IBulkOrderLockService orderLockService,
        IReadOnlyList<string> orderIds,
        string action,
        string? reason,
        CancellationToken cancellationToken)
    {
        var orders = await db.Orders
            .AsNoTracking()
            .Where(o => orderIds.Contains(o.Id))
            .Select(o => new { o.Id, o.Status })
            .ToListAsync(cancellationToken);

        var found = orders.ToDictionary(o => o.Id);
        var notFoundCount = orderIds.Count(id => !found.ContainsKey(id));
        var lockCandidates = orders
            .Where(o => o.Status != OrderStatuses.Cancelled)
            .Select(o => o.Id)
            .ToList();
        var cancelledCount = orders.Count(o => o.Status == OrderStatuses.Cancelled);

        IReadOnlyList<string> acquired = [];
        var lockFailedCount = 0;

        if (lockCandidates.Count > 0)
        {
            (acquired, lockFailedCount) = await orderLockService.TryAcquireAsync(lockCandidates, cancellationToken);

            try
            {
                if (acquired.Count > 0)
                {
                    switch (action)
                    {
                        case BulkActions.Approve:
                            await db.Orders
                                .Where(o => acquired.Contains(o.Id))
                                .ExecuteUpdateAsync(
                                    setters => setters
                                        .SetProperty(o => o.Status, OrderStatuses.Approved)
                                        .SetProperty(o => o.UpdatedAt, DateTime.UtcNow)
                                        .SetProperty(o => o.Version, o => o.Version + 1),
                                    cancellationToken);
                            break;

                        case BulkActions.Reject:
                            await db.Orders
                                .Where(o => acquired.Contains(o.Id))
                                .ExecuteUpdateAsync(
                                    setters => setters
                                        .SetProperty(o => o.Status, OrderStatuses.Rejected)
                                        .SetProperty(o => o.UpdatedAt, DateTime.UtcNow)
                                        .SetProperty(o => o.Version, o => o.Version + 1),
                                    cancellationToken);
                            break;

                        case BulkActions.Flag:
                            await ApplyFlagAsync(db, acquired, reason, cancellationToken);
                            break;
                    }
                }
            }
            finally
            {
                await orderLockService.ReleaseAsync(acquired, cancellationToken);
            }
        }

        var failed = notFoundCount + cancelledCount + lockFailedCount;
        var completed = acquired.Count;
        return (completed, failed);
    }

    private static async Task ApplyFlagAsync(
        AppDbContext db,
        IReadOnlyList<string> orderIds,
        string? reason,
        CancellationToken cancellationToken)
    {
        var flagText = reason ?? "flagged";
        var orders = await db.Orders
            .Where(o => orderIds.Contains(o.Id))
            .ToListAsync(cancellationToken);

        var updated = false;
        foreach (var order in orders)
        {
            if (order.Notes?.Contains(flagText) == true)
            {
                continue;
            }

            order.Notes = string.IsNullOrEmpty(order.Notes) ? flagText : $"{order.Notes}\n{flagText}";
            order.UpdatedAt = DateTime.UtcNow;
            order.Version++;
            updated = true;
        }

        if (updated)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task UpdateJobProgressAsync(
        AppDbContext db,
        Guid jobId,
        int completed,
        int failed,
        CancellationToken cancellationToken)
    {
        await db.Jobs
            .Where(j => j.Id == jobId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(j => j.Completed, completed)
                    .SetProperty(j => j.Failed, failed),
                cancellationToken);
    }

    private async Task FinalizeJobAsync(
        Guid jobId,
        int completed,
        int failed,
        CancellationToken cancellationToken)
    {
        var finalStatus = completed == 0 && failed > 0
            ? JobStatuses.Failed
            : JobStatuses.Completed;

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Jobs
            .Where(j => j.Id == jobId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(j => j.Status, finalStatus)
                    .SetProperty(j => j.Completed, completed)
                    .SetProperty(j => j.Failed, failed),
                cancellationToken);
    }

    private async Task MarkJobFailedAsync(Guid jobId, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Jobs
            .Where(j => j.Id == jobId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(j => j.Status, JobStatuses.Failed),
                cancellationToken);
    }
}
