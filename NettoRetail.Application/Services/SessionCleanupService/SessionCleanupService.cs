using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NettoRetail.DataAccess.Specifications;
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

                var specification = new GetSessionsWithExpiredTokenSpecification();
                await sessionRepository.DeleteManyBySpecAsync(specification, cancellationToken);
            }
        }
    }
}
