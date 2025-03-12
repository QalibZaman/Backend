using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.Services.ISessionCleanupService;

public class SessionCleanupService(IServiceScopeFactory scopeFactory, IConfiguration configuration) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(configuration.GetSection("SessionCleanup").GetValue<int>("CleanupTime"));

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_cleanupInterval, cancellationToken);

            using (var scope = _scopeFactory.CreateScope())
            {
                var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
                var allSessions = await sessionRepository.GetAllAsync(cancellationToken);
                var sessionsToDelete = allSessions.Where(s => JwtUtils.IsJwtExpired(s.Jwt)).ToList();

                await sessionRepository.DeleteRangeAsync(sessionsToDelete, cancellationToken);
            }
        }
    }
}
