namespace OrderOps.Infrastructure.Common;

public static class OrderStatuses
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Shipped = "shipped";
    public const string Delivered = "delivered";
    public const string Cancelled = "cancelled";

    private static readonly HashSet<string> Valid = new(StringComparer.Ordinal)
    {
        Pending, Approved, Rejected, Shipped, Delivered, Cancelled
    };

    public static bool IsValid(string? status) =>
        status is not null && Valid.Contains(status);
}
