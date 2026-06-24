using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderOps.Infrastructure.Data;
using OrderOps.Infrastructure.Import;

namespace OrderOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderOpsInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<CsvImporter>();

        return services;
    }
}
