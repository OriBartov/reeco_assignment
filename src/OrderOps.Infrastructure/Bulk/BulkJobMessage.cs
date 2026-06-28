namespace OrderOps.Infrastructure.Bulk;

public sealed record BulkJobMessage(
    Guid JobId,
    IReadOnlyList<string> OrderIds,
    string Action,
    string? Reason);
