using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrderOps.Api.Controllers;
using OrderOps.Api.Endpoints;
using OrderOps.Api.Json;
using OrderOps.Api.Middleware;
using OrderOps.Api.ModelBinding;
using OrderOps.Infrastructure;
using OrderOps.Infrastructure.Data;
using OrderOps.Infrastructure.Import;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:3000");

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

builder.Services.AddOrderOpsInfrastructure(builder.Configuration, connectionString);
builder.Services.Configure<ImportOptions>(builder.Configuration.GetSection(ImportOptions.SectionName));

builder.Services.AddControllers(options =>
{
    options.ValueProviderFactories.Insert(0, new SnakeCaseQueryValueProviderFactory());
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OrderOps API",
        Version = "v1",
        Description = "Procurement order-management REST API"
    });
});

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

if (corsOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            policy.WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
}

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

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (corsOrigins.Length > 0)
{
    app.UseCors("Frontend");
}

if (app.Environment.IsDevelopment() ||
    app.Environment.EnvironmentName.Equals("Docker", StringComparison.OrdinalIgnoreCase))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty;
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderOps API v1");
    });
}

app.UseMiddleware<NotFoundMiddleware>();

app.MapControllers();
app.MapEventsEndpoint();

app.MapFallbackToController(
    nameof(FallbackController.UnknownApiRoute),
    "Fallback",
    "api/{**catchAll}");

app.MapGet("/health", () => Results.Json(new { status = "ok" }, new JsonSerializerOptions
{
    PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance
}))
    .WithName("GetHealth")
    .WithTags("Health")
    .WithOpenApi();

app.Run();

