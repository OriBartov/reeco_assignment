using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace OrderOps.Infrastructure.Realtime;

public sealed class EventStreamRegistry
{
    private static readonly JsonSerializerOptions ClientEventJsonOptions = new()
    {
        PropertyNamingPolicy = null
    };

    private readonly ConcurrentDictionary<Guid, EventStreamClient> _clients = new();

    public EventStreamClient Register(string? supplierIdFilter)
    {
        var client = new EventStreamClient(supplierIdFilter);
        _clients[client.Id] = client;
        return client;
    }

    public void Unregister(Guid clientId)
    {
        _clients.TryRemove(clientId, out _);
    }

    public void Dispatch(PublishedEvent publishedEvent)
    {
        foreach (var client in _clients.Values)
        {
            if (client.SupplierIdFilter is not null &&
                !string.Equals(client.SupplierIdFilter, publishedEvent.SupplierId, StringComparison.Ordinal))
            {
                continue;
            }

            client.TryEnqueue(publishedEvent);
        }
    }

    public static string FormatSsePayload(PublishedEvent publishedEvent)
    {
        var payload = new ClientEvent(publishedEvent.Type, publishedEvent.Data);
        var json = JsonSerializer.Serialize(payload, ClientEventJsonOptions);
        return $"data: {json}\n\n";
    }

    public sealed class EventStreamClient
    {
        private readonly Channel<PublishedEvent> _channel = Channel.CreateUnbounded<PublishedEvent>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

        public EventStreamClient(string? supplierIdFilter)
        {
            Id = Guid.NewGuid();
            SupplierIdFilter = supplierIdFilter;
        }

        public Guid Id { get; }
        public string? SupplierIdFilter { get; }
        public ChannelReader<PublishedEvent> Reader => _channel.Reader;

        public void TryEnqueue(PublishedEvent publishedEvent)
        {
            _channel.Writer.TryWrite(publishedEvent);
        }

        public void Complete()
        {
            _channel.Writer.TryComplete();
        }
    }

    private sealed record ClientEvent(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("data")] object Data);
}
