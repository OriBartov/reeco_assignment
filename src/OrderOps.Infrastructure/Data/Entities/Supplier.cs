namespace OrderOps.Infrastructure.Data.Entities;

public class Supplier
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public decimal? Rating { get; set; }
    public string Country { get; set; } = null!;
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
