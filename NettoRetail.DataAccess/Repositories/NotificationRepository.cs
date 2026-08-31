using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class NotificationRepository(AppDbContext context) : BaseRepository<NotificationEntity>(context), INotificationRepository
{
    public async Task DeleteOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken)
    {
        await context.Notifications
            .Where(x => x.CreatedAt < cutoffDate)
            .ExecuteDeleteAsync(cancellationToken);
    }
}