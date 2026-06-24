namespace OrderOps.Infrastructure.Common;

public sealed record PagedResult<T>(IReadOnlyList<T> Data, int Total, int Limit, int Offset);
