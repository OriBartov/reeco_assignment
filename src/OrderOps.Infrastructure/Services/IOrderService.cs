using OrderOps.Infrastructure.Common;
using OrderOps.Infrastructure.Dtos;

namespace OrderOps.Infrastructure.Services;

public interface IOrderService
{
    Task<PagedResult<OrderListItemDto>> ListAsync(OrderListQuery query, CancellationToken cancellationToken = default);
    Task<OrderDetailDto> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<OrderDetailDto> UpdateAsync(string id, UpdateOrderRequest request, CancellationToken cancellationToken = default);
}
