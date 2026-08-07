using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services.PermissionCache;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Auth;

public record LogoutCommand(string AccessToken, Guid UserId) : IRequest;

public class LogoutCommandHandler(
    IBlacklistTokenService blacklistTokenService,
    ISessionRepository sessionRepository,
    IPermissionCacheService permissionCacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AccessToken))
        {
            throw new InvalidTokenCredentialsException("Authorization token is missing or invalid.");
        }

        var transaction = unitOfWork.BeginTransaction();

        try
        {
            var sessionSpecification = new GetSessionsByJwtSpecification(request.AccessToken);
            var session = await sessionRepository.GetAsync(sessionSpecification, cancellationToken)
                ?? throw new NotFoundException("Session was not found");

            session.IsActive = false;
            await sessionRepository.UpdateAsync(session, cancellationToken);

            var refreshToken = session.RefreshToken;
            var refreshTokenExpiresAt = session.ExpiresAt;
            await blacklistTokenService.BlacklistTokenAsync(refreshToken, refreshTokenExpiresAt, cancellationToken);

            transaction.Commit();

            permissionCacheService.RemovePermissions(session.Id);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
