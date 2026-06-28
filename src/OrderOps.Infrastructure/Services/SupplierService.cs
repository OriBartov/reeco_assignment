using Microsoft.EntityFrameworkCore;
using OrderOps.Infrastructure.Caching;
using OrderOps.Infrastructure.Common;
using OrderOps.Infrastructure.Data;
using OrderOps.Infrastructure.Dtos;
using OrderOps.Infrastructure.Exceptions;

namespace OrderOps.Infrastructure.Services;

public sealed class SupplierService(AppDbContext db, IStatsCache cache) : ISupplierService
{
    public async Task<PagedResult<SupplierListItemDto>> ListAsync(
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        var query = db.Suppliers.AsNoTracking().OrderBy(s => s.Id);

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .Skip(offset)
            .Take(limit)
            .Select(s => new SupplierListItemDto(
                s.Id,
                s.Name,
                s.Email,
                s.Rating,
                s.Country,
                s.Active,
                s.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<SupplierListItemDto>(rows, total, limit, offset);
    }

    public async Task<SupplierDetailDto> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var supplier = await db.Suppliers
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new
            {
                Supplier = s,
                OrderCount = s.Orders.Count,
                TotalRevenue = s.Orders.Sum(o => o.TotalPrice)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (supplier is null)
        {
            throw new NotFoundException($"Supplier '{id}' not found.");
        }

        return new SupplierDetailDto(
            supplier.Supplier.Id,
            supplier.Supplier.Name,
            supplier.Supplier.Email,
            supplier.Supplier.Rating,
            supplier.Supplier.Country,
            supplier.Supplier.Active,
            supplier.Supplier.CreatedAt,
            supplier.OrderCount,
            supplier.TotalRevenue);
    }

    public async Task<SupplierPerformanceDto> GetPerformanceAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = cache.SupplierPerformanceKey(id);
        var cached = await cache.GetAsync<SupplierPerformanceDto>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var exists = await db.Suppliers.AsNoTracking().AnyAsync(s => s.Id == id, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException($"Supplier '{id}' not found.");
        }

        var orders = db.Orders.AsNoTracking().Where(o => o.SupplierId == id);

        var totalOrders = await orders.CountAsync(cancellationToken);
        var avgOrderValue = totalOrders == 0
            ? 0
            : await orders.AverageAsync(o => o.TotalPrice, cancellationToken);

        var rejectedCount = await orders.CountAsync(
            o => o.Status == OrderStatuses.Rejected,
            cancellationToken);
        var rejectionRate = totalOrders == 0 ? 0 : (decimal)rejectedCount / totalOrders;

        var deliveredCount = await orders.CountAsync(
            o => o.Status == OrderStatuses.Delivered,
            cancellationToken);
        var avgDeliveryDays = deliveredCount == 0
            ? 0
            : (decimal)await orders
                .Where(o => o.Status == OrderStatuses.Delivered)
                .Select(o => (o.UpdatedAt - o.CreatedAt).TotalDays)
                .AverageAsync(cancellationToken);

        var priceConsistentCount = await orders.CountAsync(
            o => o.Product.Price != 0
                && Math.Abs(o.UnitPrice - o.Product.Price) / o.Product.Price <= 0.20m,
            cancellationToken);
        var priceConsistency = totalOrders == 0 ? 0 : (decimal)priceConsistentCount / totalOrders;

        var monthlyTrend = (await orders
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync(cancellationToken))
            .Select(x => new SupplierMonthlyTrendDto($"{x.Year:D4}-{x.Month:D2}", x.OrderCount))
            .ToList();

        var performance = new SupplierPerformanceDto(
            avgDeliveryDays,
            rejectionRate,
            avgOrderValue,
            monthlyTrend,
            priceConsistency);

        await cache.SetAsync(cacheKey, performance, cache.DefaultTtl, cancellationToken);

        return performance;
    }
}
