namespace OrderOps.Api.Models;

public sealed record ErrorResponse(string Error, string Code);

public sealed record PagedResponse<T>(IReadOnlyList<T> Data, int Total, int Limit, int Offset);
