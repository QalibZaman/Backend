using MediatR;
using NettoRetail.Application.CQRS.Users.Commands.Auth;
using NettoRetail.Application.Exceptions;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.API.Infrastructure.Middlewares;
public class SessionActivityMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IMediator mediator, ISessionRepository sessionRepository)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var token = authHeader.ToString().Split(" ")[1];

            var session = await sessionRepository.GetByJwtAsync(token);

            if (session is null || !session.IsActive)
            {
                throw new MyUnauthorizedException("Session expired or not found.");
            }

            var now = DateTime.UtcNow;
            var inactivityLimit = TimeSpan.FromHours(2);
            var updateThreshold = TimeSpan.FromMinutes(5);
            var inactivityDuration = now - session.LastActivityTime;

            if (inactivityDuration > inactivityLimit)
            {
                await mediator.Send(new LogoutCommand(token, session.AccountId));
                throw new MyUnauthorizedException("Session expired sue to inactivity");
            }

            if (inactivityDuration > updateThreshold)
            {
                session.LastActivityTime = now;
                await sessionRepository.UpdateAsync(session, context.RequestAborted);
            }
        }


        await next(context);
    }
}