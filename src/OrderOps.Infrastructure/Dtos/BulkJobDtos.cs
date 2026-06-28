using System.Text.Json.Serialization;

namespace OrderOps.Infrastructure.Dtos;

public sealed record JobProgressDto(int Total, int Completed, int Failed);

public sealed record JobStatusDto(string Status, JobProgressDto Progress);

public sealed record BulkActionJobResponse(
    [property: JsonPropertyName("jobId")] Guid JobId);

public sealed record BulkActionsJobResponse(Guid JobId);

public sealed record BulkJobResponse(
    [property: JsonPropertyName("jobId")] Guid JobId);

public sealed record BulkActionRequest(
    [property: JsonPropertyName("orderIds")] IReadOnlyList<string>? OrderIds,
    string? Action,
    string? Reason);

public sealed record BulkActionsRequest(
    [property: JsonPropertyName("order_ids")] IReadOnlyList<string>? OrderIds,
    string? Action,
    string? Reason);

public sealed record BulkRequest(
    [property: JsonPropertyName("orderIds")] IReadOnlyList<string>? OrderIds,
    string? Action,
    string? Reason);
