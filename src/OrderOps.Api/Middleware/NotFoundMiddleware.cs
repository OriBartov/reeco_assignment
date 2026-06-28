using System.Text.Json;
using OrderOps.Api.Json;
using OrderOps.Api.Models;

namespace OrderOps.Api.Middleware;

public sealed class NotFoundMiddleware(RequestDelegate next)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance
    };

    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (context.Response.HasStarted)
        {
            return;
        }

        if (context.Response.StatusCode == StatusCodes.Status404NotFound &&
            context.Response.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) != true)
        {
            context.Response.ContentType = "application/json";
            var payload = new ErrorResponse("Not found.", "NOT_FOUND");
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
        }
    }
}
