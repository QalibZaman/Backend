namespace NettoRetail.Application.Interfaces;

public interface INotificationCleanupService
{
    public Task DeleteOldNotificationsAsync(CancellationToken cancellationToken);
}