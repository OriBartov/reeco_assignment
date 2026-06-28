using StackExchange.Redis;

namespace OrderOps.Infrastructure.Bulk;

public sealed class RedisBulkOrderLockService(IConnectionMultiplexer redis) : IBulkOrderLockService
{
    private static readonly TimeSpan LockTtl = TimeSpan.FromSeconds(30);
    private const string KeyPrefix = "bulk:order:";

    public async Task<(IReadOnlyList<string> Acquired, int Failed)> TryAcquireAsync(
        IReadOnlyList<string> orderIds,
        CancellationToken cancellationToken = default)
    {
        if (orderIds.Count == 0)
        {
            return ([], 0);
        }

        var db = redis.GetDatabase();
        var acquired = new List<string>(orderIds.Count);

        foreach (var orderId in orderIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var key = KeyPrefix + orderId;
            var locked = await db.StringSetAsync(key, "1", LockTtl, When.NotExists);
            if (locked)
            {
                acquired.Add(orderId);
            }
        }

        return (acquired, orderIds.Count - acquired.Count);
    }

    public async Task ReleaseAsync(
        IReadOnlyList<string> orderIds,
        CancellationToken cancellationToken = default)
    {
        if (orderIds.Count == 0)
        {
            return;
        }

        var db = redis.GetDatabase();

        foreach (var orderId in orderIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await db.KeyDeleteAsync(KeyPrefix + orderId);
        }
    }
}
