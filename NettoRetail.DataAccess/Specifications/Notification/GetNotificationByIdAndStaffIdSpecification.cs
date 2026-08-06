using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Notification;

public class GetNotificationByIdAndStaffIdSpecification : Specification<NotificationEntity>
{
    public GetNotificationByIdAndStaffIdSpecification(Guid notificationId, Guid staffId)
        : base(model => model.Id == notificationId &&
                        model.StaffId == staffId)
    {
    }
}