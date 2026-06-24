namespace OrderOps.Infrastructure.Data.Entities;

public class Product
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string CategoryId { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
