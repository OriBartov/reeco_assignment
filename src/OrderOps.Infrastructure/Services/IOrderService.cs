using OrderOps.Infrastructure.Common;

namespace OrderOps.Infrastructure.Services;

public interface IOrderService
{
    Task<PagedResult<OrderListItemDto>> ListAsync(int limit, int offset, CancellationToken cancellationToken = default);
    Task<OrderDetailDto> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<OrderDetailDto> UpdateAsync(string id, UpdateOrderRequest request, CancellationToken cancellationToken = default);
}
