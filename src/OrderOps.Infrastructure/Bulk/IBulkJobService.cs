using OrderOps.Infrastructure.Dtos;

namespace OrderOps.Infrastructure.Bulk;

public interface IBulkJobService
{
    Task<BulkActionJobResponse> EnqueueBulkActionAsync(
        IReadOnlyList<string> orderIds,
        string action,
        string? reason,
        CancellationToken cancellationToken = default);

    Task<JobStatusDto> GetJobAsync(Guid jobId, CancellationToken cancellationToken = default);
}
