using Azure.Core;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.Services;
public class BlacklistTokenService(IBlacklistedTokenRepository blacklistedTokenRepository) : IBlacklistTokenService
{
    private IBlacklistedTokenRepository _blacklistedTokenRepository = blacklistedTokenRepository;
    public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
    {
        var allTokens = await _blacklistedTokenRepository.GetAllAsync(cancellationToken);
        var expiredTokens = allTokens.Where(t => JwtUtils.IsJwtExpired(t.Token)).ToList();
        await _blacklistedTokenRepository.DeleteRangeAsync(expiredTokens, cancellationToken);
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