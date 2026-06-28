using System.Text.Json.Serialization;

namespace OrderOps.Infrastructure.Realtime;

public sealed record ServerEvent(string Type, object Data);

public sealed record OrderUpdatedEventData(
    string Id,
    string OldStatus,
    string NewStatus,
    DateTime UpdatedAt);

public sealed record BulkCompletedEventData(
    [property: JsonPropertyName("jobId")] Guid JobId);
