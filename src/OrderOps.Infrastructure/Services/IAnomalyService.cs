using OrderOps.Infrastructure.Dtos;

namespace OrderOps.Infrastructure.Services;

public interface IAnomalyService
{
    Task<IReadOnlyList<OrderAnomalyDto>> GetAnomaliesAsync(CancellationToken cancellationToken = default);
}
