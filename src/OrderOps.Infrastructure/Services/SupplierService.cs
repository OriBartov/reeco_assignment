using Microsoft.EntityFrameworkCore;
using OrderOps.Infrastructure.Common;
using OrderOps.Infrastructure.Data;
using OrderOps.Infrastructure.Dtos;
using OrderOps.Infrastructure.Exceptions;

namespace OrderOps.Infrastructure.Services;

public sealed class SupplierService(AppDbContext db) : ISupplierService
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
}
