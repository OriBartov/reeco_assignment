namespace OrderOps.Infrastructure.Realtime;

public interface IEventPublisher
{
    Task PublishOrderUpdatedAsync(
        string orderId,
        string supplierId,
        string oldStatus,
        string newStatus,
        DateTime updatedAt,
        CancellationToken cancellationToken = default);

    Task PublishBulkCompletedAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);
}
