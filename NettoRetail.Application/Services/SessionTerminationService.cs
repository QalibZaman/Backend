using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services;
public class SessionTerminationService(
    ISessionRepository sessionRepository,
    IBlacklistTokenService blacklistTokenService)
    : ISessionTerminationService
{
    public async Task TerminateSessionsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var spec = new GetActiveSessionsByAccountIdSpecification(accountId);
        var sessions = await sessionRepository.GetAllBySpecAsync(spec, cancellationToken);

        if (!sessions.Any())
            return;

        foreach (var session in sessions)
        {
            if (!string.IsNullOrWhiteSpace(session.RefreshToken))
            {
                await blacklistTokenService.BlacklistTokenAsync(session.RefreshToken, session.ExpiresAt, cancellationToken);
            }

            session.IsActive = false;
        }

        await sessionRepository.UpdateBulkAsync(sessions, cancellationToken);
    }
}