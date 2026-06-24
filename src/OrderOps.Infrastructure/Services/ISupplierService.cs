using OrderOps.Infrastructure.Common;

namespace OrderOps.Infrastructure.Services;

public interface ISupplierService
{
    Task<PagedResult<SupplierListItemDto>> ListAsync(int limit, int offset, CancellationToken cancellationToken = default);
    Task<SupplierDetailDto> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}
