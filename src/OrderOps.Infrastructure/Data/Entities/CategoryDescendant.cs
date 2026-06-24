namespace OrderOps.Infrastructure.Data.Entities;

public class CategoryDescendant
{
    public string AncestorId { get; set; } = null!;
    public string DescendantId { get; set; } = null!;

    public Category Ancestor { get; set; } = null!;
    public Category Descendant { get; set; } = null!;
}
