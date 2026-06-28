namespace OrderOps.Infrastructure.Bulk;

public interface IBulkJobQueue
{
    ValueTask EnqueueAsync(BulkJobMessage message, CancellationToken cancellationToken = default);

    IAsyncEnumerable<BulkJobMessage> ReadAllAsync(CancellationToken cancellationToken);
}
