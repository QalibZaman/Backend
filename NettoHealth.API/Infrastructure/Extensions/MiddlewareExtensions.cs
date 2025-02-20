using NettoHealth.API.Infrastructure.Middlewares;

namespace NettoHealth.API.Infrastructure.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this WebApplication app)
    {
        return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }

    public static IApplicationBuilder UseTracing(this WebApplication app)
    {
        return app.UseMiddleware<TracingMiddleware>();
    }
}