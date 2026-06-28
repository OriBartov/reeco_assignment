using OrderOps.Infrastructure.Realtime;

namespace OrderOps.Api.Endpoints;

public static class EventsEndpoint
{
    public static void MapEventsEndpoint(this WebApplication app)
    {
        app.MapGet("/api/events", HandleEventsAsync)
            .WithName("GetEvents")
            .WithTags("Events")
            .WithOpenApi();
    }

    private static async Task HandleEventsAsync(
        HttpContext context,
        EventStreamRegistry registry,
        CancellationToken cancellationToken)
    {
        var supplierIdFilter = context.Request.Query["supplier_id"].FirstOrDefault();

        context.Response.Headers.ContentType = "text/event-stream";
        context.Response.Headers.CacheControl = "no-cache";
        context.Response.Headers.Connection = "keep-alive";

        var client = registry.Register(supplierIdFilter);

        try
        {
            await context.Response.WriteAsync(": connected\n\n", cancellationToken);
            await context.Response.Body.FlushAsync(cancellationToken);

            await foreach (var publishedEvent in client.Reader.ReadAllAsync(cancellationToken))
            {
                var payload = EventStreamRegistry.FormatSsePayload(publishedEvent);
                await context.Response.WriteAsync(payload, cancellationToken);
                await context.Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Client disconnected.
        }
        finally
        {
            client.Complete();
            registry.Unregister(client.Id);
        }
    }
}
