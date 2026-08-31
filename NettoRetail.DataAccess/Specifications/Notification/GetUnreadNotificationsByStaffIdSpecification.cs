using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Notification;

public class GetUnreadNotificationsByStaffIdSpecification :Specification<NotificationEntity>
{
    public GetUnreadNotificationsByStaffIdSpecification(Guid staffId)
        : base(model => model.StaffId == staffId && !model.IsRead)
    {
    }
}