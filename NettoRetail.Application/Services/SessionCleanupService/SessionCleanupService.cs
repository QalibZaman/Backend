using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.ISessionCleanupService;

public class SessionCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(30);

    public SessionCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(_cleanupInterval, cancellationToken);

            using (var scope = _scopeFactory.CreateScope()) // Create a new DI scope
            {
                var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();

                var specification = new GetSessionsWithExpiredTokenSpecification();
                await sessionRepository.DeleteManyBySpecAsync(specification, cancellationToken);
            }
        }
    }
}
