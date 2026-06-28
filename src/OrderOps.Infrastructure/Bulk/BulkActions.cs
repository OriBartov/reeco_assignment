namespace OrderOps.Infrastructure.Bulk;

public static class BulkActions
{
    public const string Approve = "approve";
    public const string Reject = "reject";
    public const string Flag = "flag";

    private static readonly HashSet<string> Valid = new(StringComparer.Ordinal)
    {
        Approve, Reject, Flag
    };

    public const int MaxBatchSize = 10_000;

    public static bool IsValid(string? action) =>
        action is not null && Valid.Contains(action);
}
