namespace OrderOps.Infrastructure.Dtos;

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
    int Version,
    string? ProductName = null);

public sealed record OrderListQuery(
    int Limit = 20,
    int Offset = 0,
    string? Status = null,
    string? Priority = null,
    string? SupplierId = null,
    string? Warehouse = null,
    string? DateFrom = null,
    string? DateTo = null,
    decimal? MinTotal = null,
    string? Search = null,
    string? Sort = null,
    string? Order = null)
{
    public OrderListQuery Normalize()
    {
        var limit = Limit <= 0 ? 20 : Math.Clamp(Limit, 1, 1000);
        var offset = Offset < 0 ? 0 : Offset;
        return this with { Limit = limit, Offset = offset };
    }
}

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

public sealed record UpdateOrderRequest(string? Status, string? Priority, string? Notes);
