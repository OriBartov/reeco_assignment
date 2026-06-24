namespace OrderOps.Infrastructure.Services;

public sealed record ProductListItemDto(
    string Id,
    string Name,
    string CategoryId,
    string Sku,
    decimal Price);
