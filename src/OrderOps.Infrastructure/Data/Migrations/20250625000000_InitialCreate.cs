using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderOps.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

        migrationBuilder.CreateTable(
            name: "categories",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                parent_id = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_categories", x => x.id);
                table.ForeignKey(
                    name: "fk_categories_parent_id",
                    column: x => x.parent_id,
                    principalTable: "categories",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.Sql(
            """
            ALTER TABLE categories
            ALTER CONSTRAINT fk_categories_parent_id
            DEFERRABLE INITIALLY DEFERRED;
            """);

        migrationBuilder.CreateTable(
            name: "jobs",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "text", nullable: false),
                reason = table.Column<string>(type: "text", nullable: true),
                status = table.Column<string>(type: "text", nullable: false),
                total = table.Column<int>(type: "integer", nullable: false),
                completed = table.Column<int>(type: "integer", nullable: false),
                failed = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_jobs", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "suppliers",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                email = table.Column<string>(type: "text", nullable: false),
                rating = table.Column<decimal>(type: "numeric", nullable: true),
                country = table.Column<string>(type: "text", nullable: false),
                active = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_suppliers", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "category_descendants",
            columns: table => new
            {
                ancestor_id = table.Column<string>(type: "text", nullable: false),
                descendant_id = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_category_descendants", x => new { x.ancestor_id, x.descendant_id });
                table.ForeignKey(
                    name: "FK_category_descendants_categories_ancestor_id",
                    column: x => x.ancestor_id,
                    principalTable: "categories",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_category_descendants_categories_descendant_id",
                    column: x => x.descendant_id,
                    principalTable: "categories",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "products",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                category_id = table.Column<string>(type: "text", nullable: false),
                sku = table.Column<string>(type: "text", nullable: false),
                price = table.Column<decimal>(type: "numeric", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_products", x => x.id);
                table.ForeignKey(
                    name: "FK_products_categories_category_id",
                    column: x => x.category_id,
                    principalTable: "categories",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "orders",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                supplier_id = table.Column<string>(type: "text", nullable: false),
                product_id = table.Column<string>(type: "text", nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false),
                unit_price = table.Column<decimal>(type: "numeric", nullable: false),
                total_price = table.Column<decimal>(type: "numeric", nullable: false),
                status = table.Column<string>(type: "text", nullable: false),
                priority = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                warehouse = table.Column<string>(type: "text", nullable: true),
                notes = table.Column<string>(type: "text", nullable: true),
                version = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_orders", x => x.id);
                table.ForeignKey(
                    name: "FK_orders_products_product_id",
                    column: x => x.product_id,
                    principalTable: "products",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_orders_suppliers_supplier_id",
                    column: x => x.supplier_id,
                    principalTable: "suppliers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_categories_parent_id",
            table: "categories",
            column: "parent_id");

        migrationBuilder.CreateIndex(
            name: "IX_category_descendants_descendant_id",
            table: "category_descendants",
            column: "descendant_id");

        migrationBuilder.CreateIndex(
            name: "IX_orders_created_at",
            table: "orders",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "IX_orders_product_id",
            table: "orders",
            column: "product_id");

        migrationBuilder.CreateIndex(
            name: "IX_orders_status",
            table: "orders",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "IX_orders_status_created_at",
            table: "orders",
            columns: new[] { "status", "created_at" });

        migrationBuilder.CreateIndex(
            name: "IX_orders_supplier_id",
            table: "orders",
            column: "supplier_id");

        migrationBuilder.CreateIndex(
            name: "IX_orders_total_price",
            table: "orders",
            column: "total_price");

        migrationBuilder.CreateIndex(
            name: "IX_orders_warehouse",
            table: "orders",
            column: "warehouse",
            filter: "warehouse IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_products_category_id",
            table: "products",
            column: "category_id");

        migrationBuilder.Sql(
            """
            CREATE INDEX ix_products_name_trgm
            ON products USING gin (name gin_trgm_ops);
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_products_name_trgm;");

        migrationBuilder.DropTable(name: "category_descendants");
        migrationBuilder.DropTable(name: "jobs");
        migrationBuilder.DropTable(name: "orders");
        migrationBuilder.DropTable(name: "products");
        migrationBuilder.DropTable(name: "suppliers");
        migrationBuilder.DropTable(name: "categories");

        migrationBuilder.Sql("DROP EXTENSION IF EXISTS pg_trgm;");
    }
}
