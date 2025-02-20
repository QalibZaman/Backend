namespace NettoHealth.API.Infrastructure.Middlewares;

public class TracingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext ctx)
    {
        ctx.Response.Headers["TraceId"] = ctx.TraceIdentifier;
        await next(ctx);
    }
}