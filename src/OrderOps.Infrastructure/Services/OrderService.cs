using Microsoft.EntityFrameworkCore;
using OrderOps.Infrastructure.Common;
using OrderOps.Infrastructure.Data;
using OrderOps.Infrastructure.Data.Entities;
using OrderOps.Infrastructure.Exceptions;

namespace OrderOps.Infrastructure.Services;

public sealed class OrderService(AppDbContext db) : IOrderService
{
    public async Task<PagedResult<OrderListItemDto>> ListAsync(
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        var query = db.Orders.AsNoTracking().OrderBy(o => o.Id);

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .Skip(offset)
            .Take(limit)
            .Select(o => MapListItem(o))
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

        return await GetByIdAsync(id, cancellationToken);
    }

    private static OrderListItemDto MapListItem(Order o) => new(
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
        o.Version);

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
