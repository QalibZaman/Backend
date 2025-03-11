using MediatR;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands;
public record LogoutCommand(string AccessToken) : IRequest;

public class LogoutCommandHandler(ISessionRepository sessionRepository, IBlacklistTokenService blacklistTokenService) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetSessionsByJwtSpecification(request.AccessToken);
        await sessionRepository.DeleteAsync(specification, cancellationToken);

        await blacklistTokenService.BlacklistTokenAsync(request.AccessToken, cancellationToken);
    }
}
