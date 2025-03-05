using MediatR;
using NettoRetail.Application.CQRS.Sessions.Commands;
using NettoRetail.Application.Models.Session;

namespace NettoRetail.API.Infrastructure.Middlewares;
public class SessionActivityMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx, IMediator mediator)
    {
        if (ctx.Request.Headers.ContainsKey("Authorization"))
        {
            var sessionUpdateModel = new SessionUpdateModel()
            {
                CurrentJwt = ctx.Request.Headers["Authorization"].ToString().Split(" ")[1],
                LastActivityTime = DateTime.UtcNow,
            };

            await mediator.Send(new UpdateSessionTimeCommand(sessionUpdateModel));
        }

        await next(ctx);
    }
}