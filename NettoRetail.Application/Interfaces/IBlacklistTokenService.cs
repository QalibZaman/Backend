namespace NettoRetail.Application.Interfaces;

public interface IBlacklistTokenService
{
    Task<bool> IsTokenBlacklistedAsync(string jwt, CancellationToken cancellationToken);
    Task CleanupExpiredTokensAsync(CancellationToken cancellationToken);
    Task BlacklistTokenAsync(string jwt, DateTime expiresAt, CancellationToken cancellationToken);
}