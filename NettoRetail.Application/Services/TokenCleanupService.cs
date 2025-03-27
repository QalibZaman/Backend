using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.Services;

public class TokenCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptions<TokenBlacklistOptions> tokenBlacklistOptions) : BackgroundService
{
    private readonly TokenBlacklistOptions _tokenBlacklistOptions = tokenBlacklistOptions.Value;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(_tokenBlacklistOptions.CleanupInterval), cancellationToken);

            using (var scope = _scopeFactory.CreateScope())
            {
                var blacklistService = scope.ServiceProvider.GetRequiredService<IBlacklistTokenService>();
                await blacklistService.CleanupExpiredTokensAsync(cancellationToken);
            }
        }
    }
}