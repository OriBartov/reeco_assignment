namespace OrderOps.Infrastructure.Dtos;

public sealed record StatusAggregateDto(int Count, decimal TotalValue);

public sealed record MonthAggregateDto(string Month, int OrderCount, decimal Revenue);

public sealed record TopSupplierDto(string SupplierId, string SupplierName, decimal TotalRevenue);

public sealed record WarehouseAggregateDto(string Warehouse, int Count, decimal TotalValue);

public sealed record OrderStatsDto(
    int TotalOrders,
    decimal TotalRevenue,
    decimal AvgOrderValue,
    Dictionary<string, StatusAggregateDto> ByStatus,
    IReadOnlyList<MonthAggregateDto> ByMonth,
    IReadOnlyList<TopSupplierDto> TopSuppliers,
    IReadOnlyList<WarehouseAggregateDto> ByWarehouse);
