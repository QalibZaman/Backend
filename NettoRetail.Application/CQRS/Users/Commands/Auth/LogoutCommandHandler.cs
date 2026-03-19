using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services.PermissionCache;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Sessions;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Auth;
public record LogoutCommand(string AccessToken, Guid UserId) : IRequest;

public class LogoutCommandHandler(
    IBlacklistTokenService blacklistTokenService,
    IStaffRepository accountRepository,
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

        var expiresAt = JwtUtils.GetJwtExpiration(request.AccessToken ?? throw new InvalidTokenCredentialsException("Invalid JWT or missing expiration claim."));

        // begin transaction to ensure atomicity between token blacklisting and status update.
        // this prevents partial updates in case of failure — for example, if the token is blacklisted but the account status fails to update.
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

            var accountSpecification = new GetStaffByIdSpecification(request.UserId);
            var account = await accountRepository.GetAsync(accountSpecification, cancellationToken) ?? throw new BadRequestException("Account does not exist.");

            await accountRepository.UpdateAsync(account, cancellationToken);

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
