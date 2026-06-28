using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace OrderOps.Infrastructure.Realtime;

public sealed class RedisEventSubscriberService(
    IConnectionMultiplexer redis,
    EventStreamRegistry registry,
    ILogger<RedisEventSubscriberService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = redis.GetSubscriber();

        await subscriber.SubscribeAsync(
            RedisChannel.Literal(RedisEventPublisher.ChannelName),
            (_, message) =>
            {
                if (message.IsNullOrEmpty)
                {
                    return;
                }

                try
                {
                    var redisEvent = JsonSerializer.Deserialize<RedisEventPublisher.RedisEventMessage>(
                        message.ToString());

                    if (redisEvent is null)
                    {
                        return;
                    }

                    registry.Dispatch(new PublishedEvent(
                        redisEvent.Type,
                        redisEvent.Data,
                        redisEvent.SupplierId));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to deserialize Redis event message");
                }
            });

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            await subscriber.UnsubscribeAsync(RedisChannel.Literal(RedisEventPublisher.ChannelName));
        }
    }
}
