namespace OrderOps.Infrastructure.Caching;

public interface IStatsCache
{
    string StatsKey { get; }

    TimeSpan DefaultTtl { get; }

    string SupplierPerformanceKey(string supplierId);

    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default);

    Task InvalidateStatsAsync(CancellationToken cancellationToken = default);

    Task InvalidateSupplierPerformanceAsync(string supplierId, CancellationToken cancellationToken = default);
}
