using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.NotificationType;

public class GetNotificationPreferenceByStaffIdAndTypeId : Specification<NotificationPreferenceEntity>
{
    public GetNotificationPreferenceByStaffIdAndTypeId(Guid staffId, int typeId)
        : base(notification =>
        notification.StaffId == staffId && notification.NotificationTypeId == typeId)
    {
    }
}