using Microsoft.EntityFrameworkCore;
using OrderOps.Infrastructure.Caching;
using OrderOps.Infrastructure.Data;
using OrderOps.Infrastructure.Dtos;

namespace OrderOps.Infrastructure.Services;

public sealed class StatsService(AppDbContext db, IStatsCache cache) : IStatsService
{
    public async Task<OrderStatsDto> GetOrderStatsAsync(CancellationToken cancellationToken = default)
    {
        var cached = await cache.GetAsync<OrderStatsDto>(cache.StatsKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var orders = db.Orders.AsNoTracking();

        var totalOrders = await orders.CountAsync(cancellationToken);
        var totalRevenue = totalOrders == 0
            ? 0
            : await orders.SumAsync(o => o.TotalPrice, cancellationToken);
        var avgOrderValue = totalOrders == 0 ? 0 : totalRevenue / totalOrders;

        var byStatus = await orders
            .GroupBy(o => o.Status)
            .Select(g => new
            {
                g.Key,
                Count = g.Count(),
                TotalValue = g.Sum(o => o.TotalPrice)
            })
            .ToDictionaryAsync(
                x => x.Key,
                x => new StatusAggregateDto(x.Count, x.TotalValue),
                cancellationToken);

        var byMonth = (await orders
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.TotalPrice)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync(cancellationToken))
            .Select(x => new MonthAggregateDto(
                $"{x.Year:D4}-{x.Month:D2}",
                x.OrderCount,
                x.Revenue))
            .ToList();

        var topSuppliers = await orders
            .GroupBy(o => new { o.SupplierId, o.Supplier.Name })
            .Select(g => new
            {
                g.Key.SupplierId,
                g.Key.Name,
                TotalRevenue = g.Sum(o => o.TotalPrice)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(10)
            .Select(x => new TopSupplierDto(x.SupplierId, x.Name, x.TotalRevenue))
            .ToListAsync(cancellationToken);

        var byWarehouse = (await orders
                .GroupBy(o => string.IsNullOrEmpty(o.Warehouse) ? "unassigned" : o.Warehouse)
                .Select(g => new
                {
                    Warehouse = g.Key,
                    Count = g.Count(),
                    TotalValue = g.Sum(o => o.TotalPrice)
                })
                .OrderBy(x => x.Warehouse)
                .ToListAsync(cancellationToken))
            .Select(x => new WarehouseAggregateDto(x.Warehouse, x.Count, x.TotalValue))
            .ToList();

        var stats = new OrderStatsDto(
            totalOrders,
            totalRevenue,
            avgOrderValue,
            byStatus,
            byMonth,
            topSuppliers,
            byWarehouse);

        await cache.SetAsync(cache.StatsKey, stats, cache.DefaultTtl, cancellationToken);

        return stats;
    }
}
