using System.Net;
using System.Text.Json;
using OrderOps.Api.Json;
using OrderOps.Api.Models;
using OrderOps.Infrastructure.Exceptions;

namespace OrderOps.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, error, code) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, notFound.Message, notFound.Code),
            BadRequestException badRequest => (HttpStatusCode.BadRequest, badRequest.Message, badRequest.Code),
            ConflictException conflict => (HttpStatusCode.Conflict, conflict.Message, conflict.Code),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", "INTERNAL_ERROR")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var payload = new ErrorResponse(error, code);
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}
