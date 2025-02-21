using System.Text.Json;
using NettoRetail.API.Infrastructure.Helpers.ErrorHandlers;

namespace NettoRetail.API.Infrastructure.Middlewares;

public class GlobalExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            await HandleException(ex, ctx);
        }
    }

    private static async Task HandleException(Exception ex, HttpContext ctx)
    {
        var error = ApiExceptionHandler.Handle(ex);
        var serializedError = JsonSerializer.Serialize(
            error.Result,
            new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        ctx.Response.StatusCode = error.Status;
        ctx.Response.ContentType = "application/json";

        await ctx.Response.WriteAsync(serializedError);
    }
}