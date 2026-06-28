using OrderOps.Infrastructure.Dtos;

namespace OrderOps.Infrastructure.Services;

public interface IStatsService
{
    Task<OrderStatsDto> GetOrderStatsAsync(CancellationToken cancellationToken = default);
}
