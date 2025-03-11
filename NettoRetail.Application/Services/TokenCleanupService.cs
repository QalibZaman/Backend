using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NettoRetail.Application.Interfaces;

namespace NettoRetail.Application.Services;

public class TokenCleanupService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(configuration.GetSection("TokenBlacklist").GetValue<int>("CleanupInterval"));

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_cleanupInterval, cancellationToken);

            using (var scope = _scopeFactory.CreateScope())
            {
                var blacklistService = scope.ServiceProvider.GetRequiredService<IBlacklistTokenService>();
                await blacklistService.CleanupExpiredTokensAsync(cancellationToken);
            }
        }
    }
}