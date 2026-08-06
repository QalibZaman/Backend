using NettoRetail.Application.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services;

public class NotificationCleanupService(INotificationRepository notificationRepository) : INotificationCleanupService
{
    public async Task DeleteOldNotificationsAsync(CancellationToken cancellationToken)
    {
        var cutoffDate = DateTime.UtcNow.AddMonths(-3);

        await notificationRepository.DeleteOlderThanAsync(cutoffDate, cancellationToken);
    }
}