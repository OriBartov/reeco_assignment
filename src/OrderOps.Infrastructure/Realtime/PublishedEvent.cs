namespace OrderOps.Infrastructure.Realtime;

public sealed record PublishedEvent(
    string Type,
    object Data,
    string? SupplierId = null);
