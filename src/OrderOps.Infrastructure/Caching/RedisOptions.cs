namespace OrderOps.Infrastructure.Caching;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string? ConnectionString { get; set; }
}
