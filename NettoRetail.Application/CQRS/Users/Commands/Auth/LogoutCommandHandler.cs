using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Auth;
public record LogoutCommand(string AccessToken) : IRequest;

public class LogoutCommandHandler(ISessionRepository sessionRepository, IBlacklistTokenService blacklistTokenService) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AccessToken))
        {
            throw new InvalidTokenCredentialsException("Authorization token is missing or invalid.");
        }

        var specification = new GetSessionsByJwtSpecification(request.AccessToken);
        await sessionRepository.DeleteAsync(specification, cancellationToken);

        await blacklistTokenService.BlacklistTokenAsync(request.AccessToken, cancellationToken);
    }
}
