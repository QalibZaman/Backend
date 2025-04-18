using NettoRetail.API.Infrastructure.Middlewares;

namespace NettoRetail.API.Infrastructure.Extensions;

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

    public static IApplicationBuilder UseSessionActivity(this WebApplication app)
    {
        return app.UseMiddleware<SessionActivityMiddleware>();
    }

    public static IApplicationBuilder UsePermissionHandling(this WebApplication app)
    {
        return app.UseMiddleware<PermissionMiddleware>();
    }
}