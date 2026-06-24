using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OrderOps.Infrastructure.Data;

#nullable disable

namespace OrderOps.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20250625000000_InitialCreate")]
partial class InitialCreate
{
    /// <inheritdoc />
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.11")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity("OrderOps.Infrastructure.Data.Entities.Category", b =>
            {
                b.Property<string>("Id")
                    .HasColumnType("text")
                    .HasColumnName("id");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("name");

                b.Property<string>("ParentId")
                    .HasColumnType("text")
                    .HasColumnName("parent_id");

                b.HasKey("Id");

                b.HasIndex("ParentId");

                b.ToTable("categories", (string)null);
            });

        modelBuilder.Entity("OrderOps.Infrastructure.Data.Entities.CategoryDescendant", b =>
            {
                b.Property<string>("AncestorId")
                    .HasColumnType("text")
                    .HasColumnName("ancestor_id");

                b.Property<string>("DescendantId")
                    .HasColumnType("text")
                    .HasColumnName("descendant_id");

                b.HasKey("AncestorId", "DescendantId");

                b.HasIndex("DescendantId");

                b.ToTable("category_descendants", (string)null);
            });

        modelBuilder.Entity("OrderOps.Infrastructure.Data.Entities.Job", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b.Property<string>("Action")
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("action");

                b.Property<int>("Completed")
                    .HasColumnType("integer")
                    .HasColumnName("completed");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("created_at");

                b.Property<int>("Failed")
                    .HasColumnType("integer")
                    .HasColumnName("failed");

                b.Property<string>("Reason")
                    .HasColumnType("text")
                    .HasColumnName("reason");

                b.Property<string>("Status")
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("status");

                b.Property<int>("Total")
                    .HasColumnType("integer")
                    .HasColumnName("total");

                b.HasKey("Id");

                b.ToTable("jobs", (string)null);
            });

        modelBuilder.Entity("OrderOps.Infrastructure.Data.Entities.Order", b =>
            {
                b.Property<string>("Id")
                    .HasColumnType("text")
                    .HasColumnName("id");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("created_at");

                b.Property<string>("Notes")
                    .HasColumnType("text")
                    .HasColumnName("notes");

                b.Property<string>("Priority")
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("priority");

                b.Property<string>("ProductId")
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("product_id");

                b.Property<int>("Quantity")
                    .HasColumnType("integer")
                    .HasColumnName("quantity");

                b.Property<string>("Status")
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("status");

                b.Property<string>("SupplierId")
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("supplier_id");

                b.Property<decimal>("TotalPrice")
                    .HasColumnType("numeric")
                    .HasColumnName("total_price");

                b.Property<decimal>("UnitPrice")
                    .HasColumnType("numeric")
                    .HasColumnName("unit_price");

                b.Property<DateTime>("UpdatedAt")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("updated_at");

                b.Property<int>("Version")
                    .IsConcurrencyToken()
                    .HasColumnType("integer")
                    .HasColumnName("version");

                b.Property<string>("Warehouse")
                    .HasColumnType("text")
                    .HasColumnName("warehouse");

                b.HasKey("Id");

                b.HasIndex("CreatedAt");

                b.HasIndex("ProductId");

                b.HasIndex("Status");

                b.HasIndex("SupplierId");

                b.HasIndex("TotalPrice");

                b.HasIndex("Warehouse")
                    .HasFilter("warehouse IS NOT NULL");

                b.HasIndex("Status", "CreatedAt");

                b.ToTable("orders", (string)null);
            });

        modelBuilder.Entity("OrderOps.Infrastructure.Data.Entities.Product", b =>
            {
                b.Property<string>("Id")
                    .HasColumnType("text")
                    .HasColumnName("id");

                b.Property<string>("CategoryId")
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("category_id");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("name");

                b.Property<decimal>("Price")
                    .HasColumnType("numeric")
                    .HasColumnName("price");

                b.Property<string>("Sku")
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("sku");

                b.HasKey("Id");

                b.HasIndex("CategoryId");

                b.ToTable("products", (string)null);
            });

        modelBuilder.Entity("OrderOps.Infrastructure.Data.Entities.Supplier", b =>
            {
                b.Property<string>("Id")
                    .HasColumnType("text")
                    .HasColumnName("id");

                b.Property<bool>("Active")
                    .HasColumnType("boolean")
                    .HasColumnName("active");

                b.Property<string>("Country")
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("country");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("created_at");

                b.Property<string>("Email")
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("email");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("name");

                b.Property<decimal?>("Rating")
                    .HasColumnType("numeric")
                    .HasColumnName("rating");

                b.HasKey("Id");

                b.ToTable("suppliers", (string)null);
            });

        modelBuilder.Entity("OrderOps.Infrastructure.Data.Entities.Category", b =>
            {
                b.HasOne("OrderOps.Infrastructure.Data.Entities.Category", "Parent")
                    .WithMany("Children")
                    .HasForeignKey("ParentId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_categories_parent_id");

                b.Navigation("Parent");
            });

        modelBuilder.Entity("OrderOps.Infrastructure.Data.Entities.CategoryDescendant", b =>
            {
                b.HasOne("OrderOps.Infrastructure.Data.Entities.Category", "Ancestor")
                    .WithMany()
                    .HasForeignKey("AncestorId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("OrderOps.Infrastructure.Data.Entities.Category", "Descendant")
                    .WithMany()
                    .HasForeignKey("DescendantId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Ancestor");
                b.Navigation("Descendant");
            });

        modelBuilder.Entity("OrderOps.Infrastructure.Data.Entities.Order", b =>
            {
                b.HasOne("OrderOps.Infrastructure.Data.Entities.Product", "Product")
                    .WithMany("Orders")
                    .HasForeignKey("ProductId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("OrderOps.Infrastructure.Data.Entities.Supplier", "Supplier")
                    .WithMany("Orders")
                    .HasForeignKey("SupplierId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Product");
                b.Navigation("Supplier");
            });

        modelBuilder.Entity("OrderOps.Infrastructure.Data.Entities.Product", b =>
            {
                b.HasOne("OrderOps.Infrastructure.Data.Entities.Category", "Category")
                    .WithMany("Products")
                    .HasForeignKey("CategoryId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Category");
            });

        modelBuilder.Entity("OrderOps.Infrastructure.Data.Entities.Category", b =>
            {
                b.Navigation("Children");
                b.Navigation("Products");
            });

        modelBuilder.Entity("OrderOps.Infrastructure.Data.Entities.Product", b =>
            {
                b.Navigation("Orders");
            });

        modelBuilder.Entity("OrderOps.Infrastructure.Data.Entities.Supplier", b =>
            {
                b.Navigation("Orders");
            });
#pragma warning restore 612, 618
    }
}
