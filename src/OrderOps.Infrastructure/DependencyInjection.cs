using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderOps.Infrastructure.Bulk;
using OrderOps.Infrastructure.Caching;
using OrderOps.Infrastructure.Data;
using OrderOps.Infrastructure.Import;
using OrderOps.Infrastructure.Services;
using OrderOps.Infrastructure.Workers;
using StackExchange.Redis;

namespace OrderOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderOpsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        var redisConnection = configuration[$"{RedisOptions.SectionName}:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConnection));
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
            });
            services.AddSingleton<IBulkOrderLockService, RedisBulkOrderLockService>();
        }
        else
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<IBulkOrderLockService, InMemoryBulkOrderLockService>();
        }

        services.AddSingleton<IStatsCache, StatsCache>();

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<CsvImporter>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IStatsService, StatsService>();
        services.AddScoped<IAnomalyService, AnomalyService>();

        services.AddSingleton<IBulkJobQueue, BulkJobQueue>();
        services.AddScoped<IBulkJobService, BulkJobService>();
        services.AddHostedService<BulkJobBackgroundService>();

        return services;
    }
}
