using Microsoft.EntityFrameworkCore;
using OrderOps.Infrastructure.Common;
using OrderOps.Infrastructure.Data;

namespace OrderOps.Infrastructure.Services;

public sealed class ProductService(AppDbContext db) : IProductService
{
    public async Task<PagedResult<ProductListItemDto>> ListAsync(
        int limit,
        int offset,
        string? categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            query = query.Where(p =>
                db.CategoryDescendants.Any(cd =>
                    cd.AncestorId == categoryId && cd.DescendantId == p.CategoryId));
        }

        query = query.OrderBy(p => p.Id);

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .Skip(offset)
            .Take(limit)
            .Select(p => new ProductListItemDto(
                p.Id,
                p.Name,
                p.CategoryId,
                p.Sku,
                p.Price))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListItemDto>(rows, total, limit, offset);
    }
}
