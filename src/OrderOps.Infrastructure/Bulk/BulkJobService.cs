using Microsoft.EntityFrameworkCore;
using OrderOps.Infrastructure.Data;
using OrderOps.Infrastructure.Data.Entities;
using OrderOps.Infrastructure.Dtos;
using OrderOps.Infrastructure.Exceptions;

namespace OrderOps.Infrastructure.Bulk;

public sealed class BulkJobService(AppDbContext db, IBulkJobQueue queue) : IBulkJobService
{
    public async Task<BulkActionJobResponse> EnqueueBulkActionAsync(
        IReadOnlyList<string> orderIds,
        string action,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(orderIds, action);

        var jobId = Guid.NewGuid();
        var job = new Job
        {
            Id = jobId,
            Action = action,
            Reason = reason,
            Status = JobStatuses.Processing,
            Total = orderIds.Count,
            Completed = 0,
            Failed = 0,
            CreatedAt = DateTime.UtcNow
        };

        db.Jobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        await queue.EnqueueAsync(
            new BulkJobMessage(jobId, orderIds, action, reason),
            cancellationToken);

        return new BulkActionJobResponse(jobId);
    }

    public async Task<JobStatusDto> GetJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await db.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job is null)
        {
            throw new NotFoundException($"Job '{jobId}' not found.");
        }

        return new JobStatusDto(
            job.Status,
            new JobProgressDto(job.Total, job.Completed, job.Failed));
    }

    internal static void ValidateRequest(IReadOnlyList<string> orderIds, string action)
    {
        if (orderIds.Count == 0)
        {
            throw new BadRequestException("orderIds must contain at least one order id.");
        }

        if (orderIds.Count > BulkActions.MaxBatchSize)
        {
            throw new BadRequestException($"Bulk batches cannot exceed {BulkActions.MaxBatchSize} order ids.");
        }

        if (!BulkActions.IsValid(action))
        {
            throw new BadRequestException($"Invalid bulk action: '{action}'.");
        }
    }
}
