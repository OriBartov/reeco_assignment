using System.Collections.Concurrent;
using System.Threading.Channels;

namespace OrderOps.Infrastructure.Realtime;

public sealed class EventHub
{
    private readonly ConcurrentDictionary<Guid, Subscription> _subscriptions = new();

    public (Guid Id, ChannelReader<ServerEvent> Reader) Subscribe(string? supplierIdFilter)
    {
        var channel = Channel.CreateUnbounded<ServerEvent>(new UnboundedChannelOptions
        {
            SingleReader = true,
            AllowSynchronousContinuations = false
        });

        var id = Guid.NewGuid();
        _subscriptions[id] = new Subscription(supplierIdFilter, channel.Writer);
        return (id, channel.Reader);
    }

    public void Unsubscribe(Guid id)
    {
        if (_subscriptions.TryRemove(id, out var subscription))
        {
            subscription.Writer.TryComplete();
        }
    }

    public void Publish(ServerEvent serverEvent, string? supplierId)
    {
        foreach (var subscription in _subscriptions.Values)
        {
            if (subscription.SupplierIdFilter is not null &&
                !string.Equals(subscription.SupplierIdFilter, supplierId, StringComparison.Ordinal))
            {
                continue;
            }

            subscription.Writer.TryWrite(serverEvent);
        }
    }

    private sealed record Subscription(string? SupplierIdFilter, ChannelWriter<ServerEvent> Writer);
}
