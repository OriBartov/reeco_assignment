namespace OrderOps.Infrastructure.Dtos;

public sealed record OrderAnomalyDto(
    string OrderId,
    IReadOnlyList<string> AnomalyTypes,
    string Severity);

public sealed record AnomaliesListDto(IReadOnlyList<OrderAnomalyDto> Data);
