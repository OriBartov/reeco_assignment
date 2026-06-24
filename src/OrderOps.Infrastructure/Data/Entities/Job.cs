namespace OrderOps.Infrastructure.Data.Entities;

public class Job
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public string? Reason { get; set; }
    public string Status { get; set; } = null!;
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
    public DateTime CreatedAt { get; set; }
}
