using System.Text.Json;
using System.Text.Json.Serialization;
using NettoRetail.API.Infrastructure.Helpers.ErrorHandlers;

namespace NettoRetail.API.Infrastructure.Middlewares;

public class GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            logger.LogCritical(ex.StackTrace);
            await HandleException(ex, ctx);
        }
    }

    private static async Task HandleException(Exception ex, HttpContext ctx)
    {
        if (ctx.Response.HasStarted) return;
        var error = ApiExceptionHandler.Handle(ex);
        var serializedError = JsonSerializer.Serialize(error.Result, SerializerOptions);

        ctx.Response.StatusCode = error.Status;
        ctx.Response.ContentType = "application/json";

        await ctx.Response.WriteAsync(serializedError);
    }
}