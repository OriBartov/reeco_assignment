using OrderOps.Infrastructure.Common;

namespace OrderOps.Infrastructure.Services;

public interface IProductService
{
    Task<PagedResult<ProductListItemDto>> ListAsync(
        int limit,
        int offset,
        string? categoryId,
        CancellationToken cancellationToken = default);
}
