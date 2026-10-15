using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class NotificationPreferenceRepository(AppDbContext context)
    : BaseRepository<NotificationPreferenceEntity>(context), INotificationPreferenceRepository
{
}