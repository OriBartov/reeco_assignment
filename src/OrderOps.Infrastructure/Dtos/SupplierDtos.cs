namespace OrderOps.Infrastructure.Dtos;

public sealed record SupplierListItemDto(
    string Id,
    string Name,
    string Email,
    decimal? Rating,
    string Country,
    bool Active,
    DateTime CreatedAt);

public sealed record SupplierDetailDto(
    string Id,
    string Name,
    string Email,
    decimal? Rating,
    string Country,
    bool Active,
    DateTime CreatedAt,
    int OrderCount,
    decimal TotalRevenue);
