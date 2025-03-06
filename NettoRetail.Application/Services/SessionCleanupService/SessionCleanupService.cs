using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NettoRetail.Shared.Interfaces.Repositories;

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
                var sessionsToDelete = allSessions.Where(s => IsJwtExpired(s.Jwt)).ToList();

                await sessionRepository.DeleteRangeAsync(sessionsToDelete, cancellationToken);
            }
        }
    }

    private static bool IsJwtExpired(string jwtToken)
    {
        if (string.IsNullOrEmpty(jwtToken)) return true;

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var token = handler.ReadJwtToken(jwtToken);
            var expClaim = token?.Claims.FirstOrDefault(c => c.Type == "exp");

            if (expClaim == null) return true;

            var expirationDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value)).DateTime;

            return expirationDate < DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }
}
