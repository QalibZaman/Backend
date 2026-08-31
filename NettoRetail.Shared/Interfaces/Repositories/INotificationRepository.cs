using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface INotificationRepository : IBaseRepository<NotificationEntity>
{
    public Task DeleteOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken);

}