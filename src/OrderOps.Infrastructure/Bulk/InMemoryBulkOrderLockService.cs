using System.Collections.Concurrent;

namespace OrderOps.Infrastructure.Bulk;

/// <summary>
/// Process-local lock fallback when Redis is not configured (single-instance dev).
/// </summary>
public sealed class InMemoryBulkOrderLockService : IBulkOrderLockService
{
    private static readonly TimeSpan LockTtl = TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<string, DateTime> _locks = new();

    public Task<(IReadOnlyList<string> Acquired, int Failed)> TryAcquireAsync(
        IReadOnlyList<string> orderIds,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var acquired = new List<string>(orderIds.Count);

        foreach (var orderId in orderIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            PurgeExpired(now);

            if (_locks.TryAdd(orderId, now.Add(LockTtl)))
            {
                acquired.Add(orderId);
            }
        }

        return Task.FromResult<(IReadOnlyList<string>, int)>((acquired, orderIds.Count - acquired.Count));
    }

    public Task ReleaseAsync(
        IReadOnlyList<string> orderIds,
        CancellationToken cancellationToken = default)
    {
        foreach (var orderId in orderIds)
        {
            _locks.TryRemove(orderId, out _);
        }

        return Task.CompletedTask;
    }

    private void PurgeExpired(DateTime now)
    {
        foreach (var entry in _locks)
        {
            if (entry.Value <= now)
            {
                _locks.TryRemove(entry.Key, out _);
            }
        }
    }
}
