using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.DataAccess.Specifications.InvitationToken;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.BackgroundServices;

public class PromotionCleanupService(
    IServiceProvider serviceProvider,
    ILogger<PromotionCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Promotion Cleanup Service is starting.");

        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IInventoryItemRepository>();

                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                var activateSpec = new GetItemsToActivatePromotionSpecification(today);
                var itemsToActivate = (await repository.GetAllBySpecAsync(activateSpec, stoppingToken)).ToList();

                if (itemsToActivate.Count > 0)
                {
                    foreach (var item in itemsToActivate)
                    {
                        item.IsPromoted = true;
                        item.LastUpdatedDate = DateTime.UtcNow;
                    }

                    await repository.UpdateManyAsync(itemsToActivate, stoppingToken);

                    logger.LogInformation("Activated {Count} promotions.", itemsToActivate.Count);
                }

                var deactivateSpec = new GetItemsToDeactivatePromotionSpecification(today);
                var itemsToDeactivate = (await repository.GetAllBySpecAsync(deactivateSpec, stoppingToken)).ToList();

                if (itemsToDeactivate.Count > 0)
                {
                    foreach (var item in itemsToDeactivate)
                    {
                        item.IsPromoted = false;
                        item.LastUpdatedDate = DateTime.UtcNow;
                    }

                    await repository.UpdateManyAsync(itemsToDeactivate, stoppingToken);

                    logger.LogInformation("Deactivated {Count} promotions.", itemsToDeactivate.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing promotions.");
            }

            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }
    }
}