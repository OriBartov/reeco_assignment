using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderOps.Infrastructure.Bulk;
using OrderOps.Infrastructure.Common;
using OrderOps.Infrastructure.Data;
using OrderOps.Infrastructure.Data.Entities;
using OrderOps.Infrastructure.Workers;

namespace OrderOps.Infrastructure.Tests;

public sealed class BulkJobBackgroundServiceTests
{
    [Fact]
    public async Task ProcessBatch_DoesNotOverwriteOrderCancelledAfterInitialEligibilityRead()
    {
        await using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        await SeedOrderAsync(db, OrderStatuses.Pending);

        var processBatch = typeof(BulkJobBackgroundService).GetMethod(
            "ProcessBatchAsync",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(processBatch);

        var lockService = new CancellingAfterReadLockService(db);
        var resultTask = (Task<(int Completed, int Failed)>)processBatch.Invoke(
            null,
            [db, lockService, new[] { "ord_1" }, BulkActions.Approve, null, CancellationToken.None])!;

        var result = await resultTask;
        var order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == "ord_1");

        Assert.Equal(0, result.Completed);
        Assert.Equal(1, result.Failed);
        Assert.Equal(OrderStatuses.Cancelled, order.Status);
    }

    private static async Task SeedOrderAsync(AppDbContext db, string status)
    {
        db.Categories.Add(new Category
        {
            Id = "cat_1",
            Name = "Test category"
        });
        db.Suppliers.Add(new Supplier
        {
            Id = "sup_1",
            Name = "Test supplier",
            Email = "supplier@example.com",
            Country = "US",
            Active = true,
            CreatedAt = DateTime.UtcNow
        });
        db.Products.Add(new Product
        {
            Id = "prod_1",
            Name = "Test product",
            CategoryId = "cat_1",
            Sku = "SKU-1",
            Price = 10m
        });
        db.Orders.Add(new Order
        {
            Id = "ord_1",
            SupplierId = "sup_1",
            ProductId = "prod_1",
            Quantity = 1,
            UnitPrice = 10m,
            TotalPrice = 10m,
            Status = status,
            Priority = "normal",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = 1
        });

        await db.SaveChangesAsync();
    }

    private sealed class CancellingAfterReadLockService(AppDbContext db) : IBulkOrderLockService
    {
        public async Task<(IReadOnlyList<string> Acquired, int Failed)> TryAcquireAsync(
            IReadOnlyList<string> orderIds,
            CancellationToken cancellationToken = default)
        {
            await db.Orders
                .Where(o => orderIds.Contains(o.Id))
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(o => o.Status, OrderStatuses.Cancelled)
                        .SetProperty(o => o.UpdatedAt, DateTime.UtcNow)
                        .SetProperty(o => o.Version, o => o.Version + 1),
                    cancellationToken);

            return (orderIds, 0);
        }

        public Task ReleaseAsync(
            IReadOnlyList<string> orderIds,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}