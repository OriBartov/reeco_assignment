using Microsoft.EntityFrameworkCore;
using OrderOps.Infrastructure.Data;
using OrderOps.Infrastructure.Dtos;

namespace OrderOps.Infrastructure.Services;

public sealed class AnomalyService(AppDbContext db) : IAnomalyService
{
    private const decimal PriceMismatchTolerance = 0.01m;
    private const decimal PriceSpikeMultiplier = 3m;
    private const double RiskySupplierThreshold = 0.5;

    private static readonly HashSet<string> CoreAnomalyTypes =
    [
        "price_mismatch",
        "inactive_supplier",
        "negative_quantity",
        "timestamp_anomaly",
        "price_spike"
    ];

    public async Task<IReadOnlyList<OrderAnomalyDto>> GetAnomaliesAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await db.Orders
            .AsNoTracking()
            .Select(o => new OrderScanRow(
                o.Id,
                o.SupplierId,
                o.Quantity,
                o.UnitPrice,
                o.TotalPrice,
                o.CreatedAt,
                o.UpdatedAt,
                o.Product.Price,
                o.Supplier.Active))
            .ToListAsync(cancellationToken);

        var supplierTotals = rows
            .GroupBy(r => r.SupplierId)
            .ToDictionary(g => g.Key, g => g.Count());

        var supplierCoreAnomalies = new Dictionary<string, int>();

        var flagged = new List<(string OrderId, List<string> Types)>(rows.Count);

        foreach (var row in rows)
        {
            var types = DetectCoreAnomalies(row);
            if (IsAfterHours(row.CreatedAt))
            {
                types.Add("after_hours");
            }

            if (types.Count > 0)
            {
                flagged.Add((row.OrderId, types));
            }

            var coreCount = types.Count(t => CoreAnomalyTypes.Contains(t));
            if (coreCount > 0)
            {
                supplierCoreAnomalies[row.SupplierId] =
                    supplierCoreAnomalies.GetValueOrDefault(row.SupplierId) + 1;
            }
        }

        var riskySuppliers = supplierTotals
            .Where(kvp =>
            {
                var anomalous = supplierCoreAnomalies.GetValueOrDefault(kvp.Key);
                return kvp.Value > 0 && (double)anomalous / kvp.Value > RiskySupplierThreshold;
            })
            .Select(kvp => kvp.Key)
            .ToHashSet();

        var rowById = rows.ToDictionary(r => r.OrderId);
        var results = new List<OrderAnomalyDto>(flagged.Count);

        foreach (var (orderId, types) in flagged)
        {
            if (riskySuppliers.Contains(rowById[orderId].SupplierId))
            {
                types.Add("risky_supplier");
            }

            results.Add(new OrderAnomalyDto(orderId, types, ComputeSeverity(types)));
        }

        return results
            .OrderBy(a => a.OrderId, StringComparer.Ordinal)
            .ToList();
    }

    private static List<string> DetectCoreAnomalies(OrderScanRow row)
    {
        var types = new List<string>(5);

        if (Math.Abs(row.TotalPrice - row.Quantity * row.UnitPrice) > PriceMismatchTolerance)
        {
            types.Add("price_mismatch");
        }

        if (!row.SupplierActive)
        {
            types.Add("inactive_supplier");
        }

        if (row.Quantity < 0)
        {
            types.Add("negative_quantity");
        }

        if (row.UpdatedAt < row.CreatedAt)
        {
            types.Add("timestamp_anomaly");
        }

        if (row.UnitPrice > row.ProductPrice * PriceSpikeMultiplier)
        {
            types.Add("price_spike");
        }

        return types;
    }

    private static bool IsAfterHours(DateTime createdAt)
    {
        var hour = createdAt.Hour;
        return hour >= 22 || hour < 6;
    }

    private static string ComputeSeverity(IReadOnlyList<string> types)
    {
        if (types.Contains("negative_quantity")
            || types.Contains("timestamp_anomaly")
            || types.Count >= 2)
        {
            return "high";
        }

        if (types.Contains("price_mismatch")
            || types.Contains("inactive_supplier")
            || types.Contains("price_spike"))
        {
            return "medium";
        }

        return "low";
    }

    private sealed record OrderScanRow(
        string OrderId,
        string SupplierId,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        decimal ProductPrice,
        bool SupplierActive);
}
