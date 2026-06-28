using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace OrderOps.Infrastructure.Caching;

public sealed class StatsCache(IDistributedCache cache, ILogger<StatsCache> logger) : IStatsCache
{
    public string StatsKey => "stats:v1";

    public TimeSpan DefaultTtl { get; } = TimeSpan.FromSeconds(60);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var bytes = await cache.GetAsync(key, cancellationToken);
            if (bytes is null || bytes.Length == 0)
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read cache key {CacheKey}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
            await cache.SetAsync(
                key,
                bytes,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write cache key {CacheKey}", key);
        }
    }

    public Task InvalidateStatsAsync(CancellationToken cancellationToken = default) =>
        RemoveSafeAsync(StatsKey, cancellationToken);

    public Task InvalidateSupplierPerformanceAsync(string supplierId, CancellationToken cancellationToken = default) =>
        RemoveSafeAsync(SupplierPerformanceKey(supplierId), cancellationToken);

    public string SupplierPerformanceKey(string supplierId) => $"supplier:perf:{supplierId}";

    private async Task RemoveSafeAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            await cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to remove cache key {CacheKey}", key);
        }
    }
}
