namespace OrderOps.Infrastructure.Realtime;

public sealed class EventPublisher(EventHub hub) : IEventPublisher
{
    public void PublishOrderUpdated(
        string orderId,
        string supplierId,
        string oldStatus,
        string newStatus,
        DateTime updatedAt)
    {
        if (string.Equals(oldStatus, newStatus, StringComparison.Ordinal))
        {
            return;
        }

        var payload = new ServerEvent(
            "order_updated",
            new OrderUpdatedEventData(orderId, oldStatus, newStatus, updatedAt));

        hub.Publish(payload, supplierId);
    }

    public void PublishBulkCompleted(Guid jobId)
    {
        var payload = new ServerEvent("bulk_completed", new BulkCompletedEventData(jobId));
        hub.Publish(payload, supplierId: null);
    }
}
