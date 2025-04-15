using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services.PermissionCache;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Auth;
public record LogoutCommand(string AccessToken) : IRequest;

public class LogoutCommandHandler(IBlacklistTokenService blacklistTokenService, IPermissionCacheService permissionCacheService) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AccessToken))
        {
            throw new InvalidTokenCredentialsException("Authorization token is missing or invalid.");
        }

        var expiresAt = JwtUtils.GetJwtExpiration(request.AccessToken ?? throw new InvalidTokenCredentialsException("Invalid JWT or missing expiration claim."));

        await blacklistTokenService.BlacklistTokenAsync(request.AccessToken, expiresAt!.Value, cancellationToken);

        permissionCacheService.RemovePermissions();
    }
}
