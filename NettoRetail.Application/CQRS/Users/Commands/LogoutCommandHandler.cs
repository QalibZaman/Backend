using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands;
public record LogoutCommand(string AccessToken) : IRequest;

public class LogoutCommandHandler(ISessionRepository sessionRepository, IBlacklistTokenService blacklistTokenService) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = request.AccessToken;

        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidTokenCredentialsException("Authorization token is missing or invalid.");
        }

        var specification = new GetSessionsByJwtSpecification(token);
        await sessionRepository.DeleteAsync(specification, cancellationToken);

        await blacklistTokenService.BlacklistTokenAsync(token, cancellationToken);
    }
}
