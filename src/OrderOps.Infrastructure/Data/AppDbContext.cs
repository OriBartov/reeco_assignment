using Microsoft.EntityFrameworkCore;
using OrderOps.Infrastructure.Data.Entities;

namespace OrderOps.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryDescendant> CategoryDescendants => Set<CategoryDescendant>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.ParentId).HasColumnName("parent_id");

            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_categories_parent_id");
        });

        modelBuilder.Entity<CategoryDescendant>(entity =>
        {
            entity.ToTable("category_descendants");
            entity.HasKey(e => new { e.AncestorId, e.DescendantId });
            entity.Property(e => e.AncestorId).HasColumnName("ancestor_id");
            entity.Property(e => e.DescendantId).HasColumnName("descendant_id");

            entity.HasOne(e => e.Ancestor)
                .WithMany()
                .HasForeignKey(e => e.AncestorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Descendant)
                .WithMany()
                .HasForeignKey(e => e.DescendantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.DescendantId);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("suppliers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").IsRequired();
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Country).HasColumnName("country").IsRequired();
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Sku).HasColumnName("sku").IsRequired();
            entity.Property(e => e.Price).HasColumnName("price");

            entity.HasOne(e => e.Category)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.CategoryId);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice).HasColumnName("unit_price");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.Priority).HasColumnName("priority").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Warehouse).HasColumnName("warehouse");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Version).HasColumnName("version").IsConcurrencyToken();

            entity.HasOne(e => e.Supplier)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Product)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.Status, e.CreatedAt });
            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.TotalPrice);
            entity.HasIndex(e => e.Warehouse)
                .HasFilter("warehouse IS NOT NULL");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.ToTable("jobs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action).HasColumnName("action").IsRequired();
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.Completed).HasColumnName("completed");
            entity.Property(e => e.Failed).HasColumnName("failed");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
    }
}
