namespace OrderOps.Infrastructure.Bulk;

public interface IBulkOrderLockService
{
    /// <summary>
    /// Attempts to acquire per-order locks. Returns IDs that were locked and the count that failed.
    /// </summary>
    Task<(IReadOnlyList<string> Acquired, int Failed)> TryAcquireAsync(
        IReadOnlyList<string> orderIds,
        CancellationToken cancellationToken = default);

    Task ReleaseAsync(
        IReadOnlyList<string> orderIds,
        CancellationToken cancellationToken = default);
}
