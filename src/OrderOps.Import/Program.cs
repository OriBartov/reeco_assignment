using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderOps.Infrastructure.Data;
using OrderOps.Infrastructure.Import;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
services.Configure<ImportOptions>(configuration.GetSection(ImportOptions.SectionName));
services.AddScoped<CsvImporter>();

await using var provider = services.BuildServiceProvider();

var logger = provider.GetRequiredService<ILogger<Program>>();
var db = provider.GetRequiredService<AppDbContext>();
var importer = provider.GetRequiredService<CsvImporter>();

logger.LogInformation("Applying database migrations...");
await db.Database.MigrateAsync();

logger.LogInformation("Running CSV import...");
await importer.ImportIfNeededAsync();

logger.LogInformation("Import CLI finished.");
