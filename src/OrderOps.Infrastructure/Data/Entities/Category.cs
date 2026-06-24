namespace OrderOps.Infrastructure.Data.Entities;

public class Category
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ParentId { get; set; }

    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
