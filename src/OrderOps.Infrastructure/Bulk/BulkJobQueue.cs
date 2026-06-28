using System.Threading.Channels;

namespace OrderOps.Infrastructure.Bulk;

public sealed class BulkJobQueue : IBulkJobQueue
{
    private readonly Channel<BulkJobMessage> _channel = Channel.CreateUnbounded<BulkJobMessage>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    public ValueTask EnqueueAsync(BulkJobMessage message, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(message, cancellationToken);

    public IAsyncEnumerable<BulkJobMessage> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
