namespace OrderOps.Infrastructure.Realtime;

public interface IEventPublisher
{
    void PublishOrderUpdated(
        string orderId,
        string supplierId,
        string oldStatus,
        string newStatus,
        DateTime updatedAt);

    void PublishBulkCompleted(Guid jobId);
}
