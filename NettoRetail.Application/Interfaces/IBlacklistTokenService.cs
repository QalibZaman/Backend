namespace NettoRetail.Application.Interfaces;

public interface IBlacklistTokenService
{
    Task BlacklistTokenAsync(string jwt, DateTime expiresAt, CancellationToken cancellationToken);
}