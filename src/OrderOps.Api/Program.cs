using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrderOps.Api.Json;
using OrderOps.Api.Middleware;
using OrderOps.Api.ModelBinding;
using OrderOps.Api.Models;
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

var webRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
var spaEnabled = File.Exists(Path.Combine(webRoot, "index.html"));

var exposeSwagger = app.Environment.IsDevelopment() ||
    app.Environment.EnvironmentName.Equals("Docker", StringComparison.OrdinalIgnoreCase);

if (exposeSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Root serves the SPA when wwwroot is populated (Docker image with frontend build).
        options.RoutePrefix = spaEnabled ? "swagger" : string.Empty;
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderOps API v1");
    });
}

if (spaEnabled)
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseMiddleware<NotFoundMiddleware>();

app.MapControllers();

app.MapGet("/health", () => Results.Json(new { status = "ok" }, new JsonSerializerOptions
{
    PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance
}))
    .WithName("GetHealth")
    .WithTags("Health")
    .WithOpenApi();

app.MapFallback(async (HttpContext context) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/json";
        var payload = new ErrorResponse("Not found.", "NOT_FOUND");
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance
        }));
        return;
    }

    if (spaEnabled && HttpMethods.IsGet(context.Request.Method))
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine(webRoot, "index.html"));
        return;
    }

    context.Response.StatusCode = StatusCodes.Status404NotFound;
});

app.Run();

