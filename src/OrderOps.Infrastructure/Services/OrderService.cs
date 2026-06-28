using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OrderOps.Infrastructure.Caching;
using OrderOps.Infrastructure.Common;
using OrderOps.Infrastructure.Dtos;
using OrderOps.Infrastructure.Data;
using OrderOps.Infrastructure.Data.Entities;
using OrderOps.Infrastructure.Exceptions;

namespace OrderOps.Infrastructure.Services;

public sealed class OrderService(AppDbContext db, IStatsCache statsCache) : IOrderService
{
    public async Task<PagedResult<OrderListItemDto>> ListAsync(
        OrderListQuery query,
        CancellationToken cancellationToken = default)
    {
        var limit = query.Limit;
        var offset = query.Offset;

        IQueryable<Order> orders = db.Orders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var statuses = query.Status
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var status in statuses)
            {
                if (!OrderStatuses.IsValid(status))
                {
                    throw new BadRequestException($"Invalid status value: '{status}'.");
                }
            }

            orders = orders.Where(o => statuses.Contains(o.Status));
        }

        if (!string.IsNullOrWhiteSpace(query.Priority))
        {
            orders = orders.Where(o => o.Priority == query.Priority);
        }

        if (!string.IsNullOrWhiteSpace(query.SupplierId))
        {
            orders = orders.Where(o => o.SupplierId == query.SupplierId);
        }

        if (!string.IsNullOrWhiteSpace(query.Warehouse))
        {
            orders = orders.Where(o => o.Warehouse == query.Warehouse);
        }

        var dateFrom = OrderQueryDateParser.ParseInclusiveStart(query.DateFrom);
        if (dateFrom is not null)
        {
            orders = orders.Where(o => o.CreatedAt >= dateFrom.Value);
        }

        var dateTo = OrderQueryDateParser.ParseInclusiveEnd(query.DateTo);
        if (dateTo is not null)
        {
            orders = orders.Where(o => o.CreatedAt <= dateTo.Value);
        }

        if (query.MinTotal is not null)
        {
            orders = orders.Where(o => o.TotalPrice >= query.MinTotal.Value);
        }

        var includeProductName = !string.IsNullOrWhiteSpace(query.Search);
        if (includeProductName)
        {
            var pattern = $"%{query.Search}%";
            orders = orders.Where(o => EF.Functions.ILike(o.Product.Name, pattern));
        }

        orders = ApplySort(orders, query.Sort, query.Order);

        var total = await orders.CountAsync(cancellationToken);
        var rows = await orders
            .Skip(offset)
            .Take(limit)
            .Select(ListItemProjection(includeProductName))
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderListItemDto>(rows, total, limit, offset);
    }

    public async Task<OrderDetailDto> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.Supplier)
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException($"Order '{id}' not found.");
        }

        return MapDetail(order);
    }

    public async Task<OrderDetailDto> UpdateAsync(
        string id,
        UpdateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Status is not null && !OrderStatuses.IsValid(request.Status))
        {
            throw new BadRequestException($"Invalid status value: '{request.Status}'.");
        }

        var order = await db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException($"Order '{id}' not found.");
        }

        if (order.Status == OrderStatuses.Cancelled)
        {
            throw new ConflictException("Cannot update an order that is already cancelled.");
        }

        var expectedVersion = order.Version;
        var newStatus = request.Status ?? order.Status;
        var newPriority = request.Priority ?? order.Priority;
        var updatedAt = DateTime.UtcNow;

        var rowsAffected = await db.Orders
            .Where(o => o.Id == id && o.Version == expectedVersion && o.Status != OrderStatuses.Cancelled)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(o => o.Status, newStatus)
                    .SetProperty(o => o.Priority, newPriority)
                    .SetProperty(o => o.UpdatedAt, updatedAt)
                    .SetProperty(o => o.Version, expectedVersion + 1),
                cancellationToken);

        if (rowsAffected == 0)
        {
            var current = await db.Orders
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Select(o => o.Status)
                .FirstOrDefaultAsync(cancellationToken);

            if (current == OrderStatuses.Cancelled)
            {
                throw new ConflictException("Cannot update an order that is already cancelled.");
            }

            throw new ConflictException("The order was modified by another request. Please retry.");
        }

        if (!string.Equals(newStatus, order.Status, StringComparison.Ordinal))
        {
            await statsCache.InvalidateStatsAsync(cancellationToken);
        }

        return await GetByIdAsync(id, cancellationToken);
    }

    private static Expression<Func<Order, OrderListItemDto>> ListItemProjection(bool includeProductName) =>
        includeProductName ? ListItemWithProductName : ListItemWithoutProductName;

    private static Expression<Func<Order, OrderListItemDto>> ListItemWithoutProductName =>
        o => new OrderListItemDto(
            o.Id,
            o.SupplierId,
            o.ProductId,
            o.Quantity,
            o.UnitPrice,
            o.TotalPrice,
            o.Status,
            o.Priority,
            o.CreatedAt,
            o.UpdatedAt,
            o.Warehouse,
            o.Notes,
            o.Version,
            null);

    private static Expression<Func<Order, OrderListItemDto>> ListItemWithProductName =>
        o => new OrderListItemDto(
            o.Id,
            o.SupplierId,
            o.ProductId,
            o.Quantity,
            o.UnitPrice,
            o.TotalPrice,
            o.Status,
            o.Priority,
            o.CreatedAt,
            o.UpdatedAt,
            o.Warehouse,
            o.Notes,
            o.Version,
            o.Product.Name);

    private static IQueryable<Order> ApplySort(IQueryable<Order> query, string? sort, string? order)
    {
        var descending = order?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;

        return (sort?.ToLowerInvariant()) switch
        {
            "created_at" => descending
                ? query.OrderByDescending(o => o.CreatedAt)
                : query.OrderBy(o => o.CreatedAt),
            "updated_at" => descending
                ? query.OrderByDescending(o => o.UpdatedAt)
                : query.OrderBy(o => o.UpdatedAt),
            "total_price" => descending
                ? query.OrderByDescending(o => o.TotalPrice)
                : query.OrderBy(o => o.TotalPrice),
            "unit_price" => descending
                ? query.OrderByDescending(o => o.UnitPrice)
                : query.OrderBy(o => o.UnitPrice),
            "quantity" => descending
                ? query.OrderByDescending(o => o.Quantity)
                : query.OrderBy(o => o.Quantity),
            "status" => descending
                ? query.OrderByDescending(o => o.Status)
                : query.OrderBy(o => o.Status),
            "priority" => descending
                ? query.OrderByDescending(o => o.Priority)
                : query.OrderBy(o => o.Priority),
            "warehouse" => descending
                ? query.OrderByDescending(o => o.Warehouse)
                : query.OrderBy(o => o.Warehouse),
            null or "" => query.OrderBy(o => o.CreatedAt).ThenBy(o => o.Id),
            _ => query.OrderBy(o => o.CreatedAt).ThenBy(o => o.Id),
        };
    }

    private static OrderDetailDto MapDetail(Order o) => new(
        o.Id,
        o.SupplierId,
        o.ProductId,
        o.Quantity,
        o.UnitPrice,
        o.TotalPrice,
        o.Status,
        o.Priority,
        o.CreatedAt,
        o.UpdatedAt,
        o.Warehouse,
        o.Notes,
        o.Version,
        o.Supplier.Name,
        o.Product.Name);
}
