using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.Services;
public class SessionTerminationService(
    ISessionRepository sessionRepository,
    IBlacklistTokenService blacklistTokenService)
    : ISessionTerminationService
{
    public async Task TerminateSessionsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var spec = new GetSessionsByAccountIdSpecification(accountId);
        var sessions = await sessionRepository.GetAllBySpecAsync(spec, cancellationToken);

        foreach (var session in sessions)
        {
            if (!string.IsNullOrWhiteSpace(session.Jwt))
            {
                var expiresAt = JwtUtils.GetJwtExpiration(session.Jwt);
                if (expiresAt.HasValue)
                {
                    await blacklistTokenService.BlacklistTokenAsync(session.Jwt, expiresAt.Value, cancellationToken);
                }
            }
        }
        await sessionRepository.DeleteManyBySpecAsync(spec, cancellationToken);
    }
}
