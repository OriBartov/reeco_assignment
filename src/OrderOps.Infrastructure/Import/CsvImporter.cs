using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderOps.Infrastructure.Data;
using OrderOps.Infrastructure.Data.Entities;

namespace OrderOps.Infrastructure.Import;

public class CsvImporter(
    AppDbContext db,
    IOptions<ImportOptions> options,
    ILogger<CsvImporter> logger)
{
    private const int BatchSize = 1000;
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public async Task ImportIfNeededAsync(CancellationToken cancellationToken = default)
    {
        var orderCount = await db.Orders.CountAsync(cancellationToken);
        if (orderCount == 50_000)
        {
            logger.LogInformation("Database already contains {Count} orders; skipping CSV import.", orderCount);
            return;
        }

        var hasPartialData = orderCount > 0 ||
            await db.Categories.AnyAsync(cancellationToken) ||
            await db.Suppliers.AnyAsync(cancellationToken) ||
            await db.Products.AnyAsync(cancellationToken);

        if (hasPartialData)
        {
            logger.LogWarning(
                "Database contains partial seed data (orders: {OrderCount}). Clearing tables before import.",
                orderCount);
            await ClearImportTablesAsync(cancellationToken);
        }

        await ImportAsync(cancellationToken);
    }

    public async Task ImportAsync(CancellationToken cancellationToken = default)
    {
        var dataPath = ResolveDataPath();
        logger.LogInformation("Starting CSV import from {DataPath}", dataPath);

        var categories = await ReadCategoriesAsync(Path.Combine(dataPath, "categories.csv"), cancellationToken);
        await ImportCategoriesAsync(categories, cancellationToken);

        var parentById = categories.ToDictionary(c => c.Id, c => c.ParentId);
        await BuildCategoryClosureAsync(parentById, cancellationToken);

        await ImportSuppliersAsync(Path.Combine(dataPath, "suppliers.csv"), cancellationToken);
        await ImportProductsAsync(Path.Combine(dataPath, "products.csv"), cancellationToken);
        await ImportOrdersAsync(Path.Combine(dataPath, "orders.csv"), cancellationToken);

        await db.Database.ExecuteSqlRawAsync("ANALYZE;", cancellationToken);

        await LogTableCountsAsync(cancellationToken);
        logger.LogInformation("CSV import completed.");
    }

    private string ResolveDataPath()
    {
        var configured = options.Value.DataPath;
        if (Path.IsPathRooted(configured))
        {
            return configured;
        }

        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), configured));
    }

    private async Task ClearImportTablesAsync(CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            TRUNCATE TABLE category_descendants, orders, products, suppliers, categories RESTART IDENTITY CASCADE;
            """,
            cancellationToken);
    }

    private async Task ImportCategoriesAsync(
        IReadOnlyList<CategoryRow> rows,
        CancellationToken cancellationToken)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var pending = new List<CategoryRow>();

        foreach (var row in rows)
        {
            if (!seen.Add(row.Id))
            {
                logger.LogWarning("Duplicate category id {CategoryId}; skipping row.", row.Id);
                continue;
            }

            pending.Add(row);
        }

        var inserted = new HashSet<string>(StringComparer.Ordinal);
        var pass = 0;

        while (pending.Count > 0)
        {
            pass++;
            var batch = new List<Category>();
            var remaining = new List<CategoryRow>();

            foreach (var row in pending)
            {
                if (string.IsNullOrWhiteSpace(row.ParentId) || inserted.Contains(row.ParentId))
                {
                    batch.Add(new Category
                    {
                        Id = row.Id,
                        Name = row.Name,
                        ParentId = string.IsNullOrWhiteSpace(row.ParentId) ? null : row.ParentId
                    });
                }
                else
                {
                    remaining.Add(row);
                }
            }

            if (batch.Count == 0)
            {
                await InsertCyclicCategoriesAsync(remaining, inserted, cancellationToken);
                pending = [];
                continue;
            }

            db.Categories.AddRange(batch);
            await db.SaveChangesAsync(cancellationToken);

            foreach (var category in batch)
            {
                inserted.Add(category.Id);
            }

            pending = remaining;
        }

        logger.LogInformation("Imported {Count} categories in {Passes} pass(es).", inserted.Count, pass);
    }

    private async Task InsertCyclicCategoriesAsync(
        IReadOnlyList<CategoryRow> rows,
        ISet<string> inserted,
        CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
        {
            return;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.Database.ExecuteSqlRawAsync(
            "SET CONSTRAINTS fk_categories_parent_id DEFERRED;",
            cancellationToken);

        foreach (var row in rows)
        {
            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO categories (id, name, parent_id)
                VALUES ({0}, {1}, {2})
                ON CONFLICT (id) DO NOTHING;
                """,
                [row.Id, row.Name, string.IsNullOrWhiteSpace(row.ParentId) ? (object)DBNull.Value : row.ParentId],
                cancellationToken);
            inserted.Add(row.Id);
        }

        await transaction.CommitAsync(cancellationToken);

        logger.LogWarning(
            "Inserted {Count} categories with deferred parent FK (likely cyclic hierarchy).",
            rows.Count);
    }

    private async Task BuildCategoryClosureAsync(
        IReadOnlyDictionary<string, string?> parentById,
        CancellationToken cancellationToken)
    {
        var pairs = new HashSet<(string AncestorId, string DescendantId)>();

        foreach (var descendantId in parentById.Keys)
        {
            var visitedOnPath = new HashSet<string>(StringComparer.Ordinal);
            var current = descendantId;

            while (true)
            {
                pairs.Add((current, descendantId));

                if (!parentById.TryGetValue(current, out var parentId) ||
                    string.IsNullOrWhiteSpace(parentId))
                {
                    break;
                }

                if (visitedOnPath.Contains(parentId))
                {
                    logger.LogWarning(
                        "Category cycle detected involving {CategoryId}; stopping ancestor expansion for descendant {DescendantId}.",
                        parentId,
                        descendantId);
                    break;
                }

                visitedOnPath.Add(current);
                current = parentId;
            }
        }

        var closureRows = pairs
            .Select(p => new CategoryDescendant { AncestorId = p.AncestorId, DescendantId = p.DescendantId })
            .ToList();

        for (var i = 0; i < closureRows.Count; i += BatchSize)
        {
            var batch = closureRows.Skip(i).Take(BatchSize).ToList();
            db.CategoryDescendants.AddRange(batch);
            await db.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Built {Count} category_descendants rows.", closureRows.Count);
    }

    private async Task ImportSuppliersAsync(string filePath, CancellationToken cancellationToken)
    {
        var rows = await ReadSuppliersAsync(filePath, cancellationToken);
        var seen = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < rows.Count; i += BatchSize)
        {
            var batch = new List<Supplier>();

            foreach (var row in rows.Skip(i).Take(BatchSize))
            {
                if (!seen.Add(row.Id))
                {
                    logger.LogWarning("Duplicate supplier id {SupplierId}; skipping row.", row.Id);
                    continue;
                }

                batch.Add(new Supplier
                {
                    Id = row.Id,
                    Name = row.Name,
                    Email = row.Email,
                    Rating = row.Rating,
                    Country = row.Country,
                    Active = row.Active,
                    CreatedAt = row.CreatedAt
                });
            }

            if (batch.Count == 0)
            {
                continue;
            }

            db.Suppliers.AddRange(batch);
            await db.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Imported {Count} suppliers.", seen.Count);
    }

    private async Task ImportProductsAsync(string filePath, CancellationToken cancellationToken)
    {
        var rows = await ReadProductsAsync(filePath, cancellationToken);
        var seen = new HashSet<string>(StringComparer.Ordinal);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.Database.ExecuteSqlRawAsync(
            "SET LOCAL session_replication_role = replica;",
            cancellationToken);

        try
        {
            for (var i = 0; i < rows.Count; i += BatchSize)
            {
                var batch = new List<Product>();

                foreach (var row in rows.Skip(i).Take(BatchSize))
                {
                    if (!seen.Add(row.Id))
                    {
                        logger.LogWarning("Duplicate product id {ProductId}; skipping row.", row.Id);
                        continue;
                    }

                    batch.Add(new Product
                    {
                        Id = row.Id,
                        Name = row.Name,
                        CategoryId = row.CategoryId,
                        Sku = row.Sku,
                        Price = row.Price
                    });
                }

                if (batch.Count == 0)
                {
                    continue;
                }

                db.Products.AddRange(batch);
                await db.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        logger.LogInformation("Imported {Count} products.", seen.Count);
    }

    private async Task ImportOrdersAsync(string filePath, CancellationToken cancellationToken)
    {
        var rows = await ReadOrdersAsync(filePath, cancellationToken);
        var seen = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < rows.Count; i += BatchSize)
        {
            var batch = new List<Order>();

            foreach (var row in rows.Skip(i).Take(BatchSize))
            {
                if (!seen.Add(row.Id))
                {
                    logger.LogWarning("Duplicate order id {OrderId}; skipping row.", row.Id);
                    continue;
                }

                batch.Add(new Order
                {
                    Id = row.Id,
                    SupplierId = row.SupplierId,
                    ProductId = row.ProductId,
                    Quantity = row.Quantity,
                    UnitPrice = row.UnitPrice,
                    TotalPrice = row.TotalPrice,
                    Status = row.Status,
                    Priority = row.Priority,
                    CreatedAt = row.CreatedAt,
                    UpdatedAt = row.UpdatedAt,
                    Warehouse = row.Warehouse,
                    Notes = row.Notes,
                    Version = 1
                });
            }

            if (batch.Count == 0)
            {
                continue;
            }

            db.Orders.AddRange(batch);
            await db.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Imported {Count} orders.", seen.Count);
    }

    private async Task LogTableCountsAsync(CancellationToken cancellationToken)
    {
        var categories = await db.Categories.CountAsync(cancellationToken);
        var suppliers = await db.Suppliers.CountAsync(cancellationToken);
        var products = await db.Products.CountAsync(cancellationToken);
        var orders = await db.Orders.CountAsync(cancellationToken);
        var descendants = await db.CategoryDescendants.CountAsync(cancellationToken);

        logger.LogInformation(
            "Final row counts — categories: {Categories}, category_descendants: {Descendants}, suppliers: {Suppliers}, products: {Products}, orders: {Orders}",
            categories,
            descendants,
            suppliers,
            products,
            orders);
    }

    private static async Task<IReadOnlyList<CategoryRow>> ReadCategoriesAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        var rows = new List<CategoryRow>();
        await foreach (var fields in ReadCsvRowsAsync(filePath, cancellationToken))
        {
            rows.Add(new CategoryRow(fields[0], fields[1], NullIfEmpty(fields[2])));
        }

        return rows;
    }

    private static async Task<IReadOnlyList<SupplierRow>> ReadSuppliersAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        var rows = new List<SupplierRow>();
        await foreach (var fields in ReadCsvRowsAsync(filePath, cancellationToken))
        {
            rows.Add(new SupplierRow(
                fields[0],
                fields[1],
                fields[2],
                ParseNullableDecimal(fields[3]),
                fields[4],
                ParseBool(fields[5]),
                ParseDateTime(fields[6])));
        }

        return rows;
    }

    private static async Task<IReadOnlyList<ProductRow>> ReadProductsAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        var rows = new List<ProductRow>();
        await foreach (var fields in ReadCsvRowsAsync(filePath, cancellationToken))
        {
            rows.Add(new ProductRow(fields[0], fields[1], fields[2], fields[3], ParseDecimal(fields[4])));
        }

        return rows;
    }

    private static async Task<IReadOnlyList<OrderRow>> ReadOrdersAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        var rows = new List<OrderRow>();
        await foreach (var fields in ReadCsvRowsAsync(filePath, cancellationToken))
        {
            rows.Add(new OrderRow(
                fields[0],
                fields[1],
                fields[2],
                ParseInt(fields[3]),
                ParseDecimal(fields[4]),
                ParseDecimal(fields[5]),
                fields[6],
                fields[7],
                ParseDateTime(fields[8]),
                ParseDateTime(fields[9]),
                NullIfEmpty(fields[10]),
                NullIfEmpty(fields[11])));
        }

        return rows;
    }

    private static async IAsyncEnumerable<string[]> ReadCsvRowsAsync(
        string filePath,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var isHeader = true;

        await foreach (var line in ReadLinesAsync(filePath, cancellationToken))
        {
            if (isHeader)
            {
                isHeader = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            yield return ParseCsvLine(line);
        }
    }

    private static async IAsyncEnumerable<string> ReadLinesAsync(
        string filePath,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                yield break;
            }

            yield return line;
        }
    }

    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }

                continue;
            }

            if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == ',')
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return fields.ToArray();
    }

    private static string? NullIfEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private static bool ParseBool(string value) =>
        string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

    private static int ParseInt(string value) =>
        int.Parse(value, Invariant);

    private static decimal ParseDecimal(string value) =>
        decimal.Parse(value, Invariant);

    private static decimal? ParseNullableDecimal(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : decimal.Parse(value, Invariant);

    private static DateTime ParseDateTime(string value) =>
        DateTime.Parse(value, Invariant, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

    private sealed record CategoryRow(string Id, string Name, string? ParentId);

    private sealed record SupplierRow(
        string Id,
        string Name,
        string Email,
        decimal? Rating,
        string Country,
        bool Active,
        DateTime CreatedAt);

    private sealed record ProductRow(string Id, string Name, string CategoryId, string Sku, decimal Price);

    private sealed record OrderRow(
        string Id,
        string SupplierId,
        string ProductId,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice,
        string Status,
        string Priority,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        string? Warehouse,
        string? Notes);
}
