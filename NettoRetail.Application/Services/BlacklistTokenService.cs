using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Token;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services;
public class BlacklistTokenService(IBlacklistedTokenRepository blacklistedTokenRepository) : IBlacklistTokenService
{
    private IBlacklistedTokenRepository _blacklistedTokenRepository = blacklistedTokenRepository;
    public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
    {
        var expiredTokensSpec = new GetExpiredTokensSpecification();
        await _blacklistedTokenRepository.DeleteManyBySpecAsync(expiredTokensSpec, cancellationToken);
    }

    public async Task<bool> IsTokenBlacklistedAsync(string jwt, CancellationToken cancellationToken)
    {
        var specification = new GetBlackListedTokenByJwtSpecification(jwt);
        var token = await _blacklistedTokenRepository.GetAsync(specification, cancellationToken);
        return token != null;
    }

    public async Task BlacklistTokenAsync(string jwt, CancellationToken cancellationToken)
    {
        if (await IsTokenBlacklistedAsync(jwt, cancellationToken))
        {
            return; // Token already blacklisted, no need to add it again
        }

        var blacklistedToken = new BlacklistedTokenEntity
        {
            Token = jwt,
            BlacklistedAt = DateTime.UtcNow
        };

        await _blacklistedTokenRepository.AddAsync(blacklistedToken, cancellationToken);
    }
}