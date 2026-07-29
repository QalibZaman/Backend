using NettoRetail.Application.Interfaces;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services;

public class BlacklistTokenService(IBlacklistedTokenRepository blacklistedTokenRepository) : IBlacklistTokenService
{
    private readonly IBlacklistedTokenRepository _blacklistedTokenRepository = blacklistedTokenRepository;

    public async Task BlacklistTokenAsync(string jwt, DateTime expiresAt, CancellationToken cancellationToken)
    {
        var blacklistedToken = new BlacklistedTokenEntity
        {
            Token = jwt,
            BlacklistedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
        };

        await _blacklistedTokenRepository.AddAsync(blacklistedToken, cancellationToken);
    }
}