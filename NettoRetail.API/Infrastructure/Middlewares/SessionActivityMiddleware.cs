using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.Users.Commands.Auth;
using NettoRetail.Application.Exceptions;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;

namespace NettoRetail.API.Infrastructure.Middlewares;
public class SessionActivityMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IMediator mediator, ISessionRepository sessionRepository, IOptions<SessionTrackingOptions> sessionOptions)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            if (!context.IsTempToken())
            {
                var token = authHeader.ToString().Split(" ")[1];

                var session = await sessionRepository.GetByJwtAsync(token);

                if (session is null || !session.IsActive)
                {
                    throw new MyUnauthorizedException("Session expired or not found.");
                }

                var now = DateTime.UtcNow;
                var inactivityLimit = TimeSpan.FromMinutes(sessionOptions.Value.InactivityTimeoutMinutes);
                var updateThreshold = TimeSpan.FromMinutes(sessionOptions.Value.UpdateThresholdMinutes);
                var inactivityDuration = now - session.LastActivityTime;

                if (inactivityDuration > inactivityLimit)
                {
                    await mediator.Send(new LogoutCommand(token, session.AccountId));
                    throw new MyUnauthorizedException("Session expired due to inactivity");
                }

                if (inactivityDuration > updateThreshold)
                {
                    session.LastActivityTime = now;
                    await sessionRepository.UpdateAsync(session, context.RequestAborted);
                }
            }
        }

        await next(context);
    }
}