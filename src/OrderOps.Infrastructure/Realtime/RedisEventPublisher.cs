using System.Text.Json;
using System.Text.Json.Serialization;
using StackExchange.Redis;

namespace OrderOps.Infrastructure.Realtime;

public sealed class RedisEventPublisher(IConnectionMultiplexer redis) : IEventPublisher
{
    public const string ChannelName = "events";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null
    };

    public async Task PublishOrderUpdatedAsync(
        string orderId,
        string supplierId,
        string oldStatus,
        string newStatus,
        DateTime updatedAt,
        CancellationToken cancellationToken = default)
    {
        var publishedEvent = new PublishedEvent(
            EventTypes.OrderUpdated,
            new OrderUpdatedPayload(orderId, oldStatus, newStatus, updatedAt),
            supplierId);

        await PublishAsync(publishedEvent, cancellationToken);
    }

    public async Task PublishBulkCompletedAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var publishedEvent = new PublishedEvent(
            EventTypes.BulkCompleted,
            new BulkCompletedPayload(jobId));

        await PublishAsync(publishedEvent, cancellationToken);
    }

    private async Task PublishAsync(PublishedEvent publishedEvent, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Serialize(new RedisEventMessage(
            publishedEvent.Type,
            publishedEvent.Data,
            publishedEvent.SupplierId), JsonOptions);

        var subscriber = redis.GetSubscriber();
        await subscriber.PublishAsync(RedisChannel.Literal(ChannelName), message);
    }

    internal sealed record RedisEventMessage(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("data")] object Data,
        [property: JsonPropertyName("supplier_id")] string? SupplierId);
}
