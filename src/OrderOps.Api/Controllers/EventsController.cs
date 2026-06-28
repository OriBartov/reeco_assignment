using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using OrderOps.Api.Json;
using OrderOps.Infrastructure.Realtime;

namespace OrderOps.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController(EventHub eventHub) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance
    };

    [HttpGet]
    public async Task Stream([FromQuery(Name = "supplier_id")] string? supplierId, CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";
        Response.ContentType = "text/event-stream";
        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

        var (subscriptionId, reader) = eventHub.Subscribe(supplierId);

        try
        {
            await Response.StartAsync(cancellationToken);
            await Response.WriteAsync(": connected\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);

            await foreach (var serverEvent in reader.ReadAllAsync(cancellationToken))
            {
                var payload = JsonSerializer.Serialize(serverEvent, JsonOptions);
                await Response.WriteAsync($"data: {payload}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        finally
        {
            eventHub.Unsubscribe(subscriptionId);
        }
    }
}
