using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.NotificationType;

public class GetNotificationPreferenceByStaffId : Specification<NotificationPreferenceEntity>
{
    public GetNotificationPreferenceByStaffId(Guid staffId)
        : base(notificationPreference =>
        notificationPreference.StaffId == staffId)
    {
        AddInclude(notificationPreference => notificationPreference.NotificationType);
    }
}