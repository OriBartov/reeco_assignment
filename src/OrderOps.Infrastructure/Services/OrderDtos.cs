namespace OrderOps.Infrastructure.Services;

public sealed record OrderListItemDto(
    string Id,
    string SupplierId,
    string ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? Warehouse,
    string? Notes,
    int Version);

public sealed record OrderDetailDto(
    string Id,
    string SupplierId,
    string ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? Warehouse,
    string? Notes,
    int Version,
    string SupplierName,
    string ProductName);

public sealed record UpdateOrderRequest(string? Status, string? Priority);
