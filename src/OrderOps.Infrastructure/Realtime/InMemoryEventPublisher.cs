namespace OrderOps.Infrastructure.Realtime;

public sealed class InMemoryEventPublisher(EventStreamRegistry registry) : IEventPublisher
{
    public Task PublishOrderUpdatedAsync(
        string orderId,
        string supplierId,
        string oldStatus,
        string newStatus,
        DateTime updatedAt,
        CancellationToken cancellationToken = default)
    {
        registry.Dispatch(new PublishedEvent(
            EventTypes.OrderUpdated,
            new OrderUpdatedPayload(orderId, oldStatus, newStatus, updatedAt),
            supplierId));

        return Task.CompletedTask;
    }

    public Task PublishBulkCompletedAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        registry.Dispatch(new PublishedEvent(
            EventTypes.BulkCompleted,
            new BulkCompletedPayload(jobId)));

        return Task.CompletedTask;
    }
}
