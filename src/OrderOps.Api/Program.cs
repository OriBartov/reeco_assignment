using Microsoft.EntityFrameworkCore;
using OrderOps.Infrastructure.Data;
using OrderOps.Infrastructure.Import;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:3000");

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<ImportOptions>(builder.Configuration.GetSection(ImportOptions.SectionName));
builder.Services.AddScoped<CsvImporter>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var importer = scope.ServiceProvider.GetRequiredService<CsvImporter>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Applying database migrations...");
    await db.Database.MigrateAsync();

    if (builder.Configuration.GetValue<bool>($"{ImportOptions.SectionName}:RunOnStartup"))
    {
        await importer.ImportIfNeededAsync();
    }
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
