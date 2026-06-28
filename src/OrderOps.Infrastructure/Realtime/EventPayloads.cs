using System.Text.Json.Serialization;

namespace OrderOps.Infrastructure.Realtime;

public sealed record OrderUpdatedPayload(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("old_status")] string OldStatus,
    [property: JsonPropertyName("new_status")] string NewStatus,
    [property: JsonPropertyName("updated_at")] DateTime UpdatedAt);

public sealed record BulkCompletedPayload(
    [property: JsonPropertyName("jobId")] Guid JobId);
