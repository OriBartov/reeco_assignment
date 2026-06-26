using OrderOps.Infrastructure.Exceptions;

namespace OrderOps.Infrastructure.Services;

internal static class OrderQueryDateParser
{
    public static DateTime? ParseInclusiveStart(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!DateOnly.TryParse(value, out var date))
        {
            throw new BadRequestException($"Invalid date_from value: '{value}'.");
        }

        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    public static DateTime? ParseInclusiveEnd(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!DateOnly.TryParse(value, out var date))
        {
            throw new BadRequestException($"Invalid date_to value: '{value}'.");
        }

        return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999, DateTimeKind.Utc);
    }
}
