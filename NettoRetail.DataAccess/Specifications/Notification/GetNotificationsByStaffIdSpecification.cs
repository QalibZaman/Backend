using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Notification;

public class GetNotificationsByStaffIdSpecification: Specification<NotificationEntity>
{
    public GetNotificationsByStaffIdSpecification(Guid id)
        : base(model => model.StaffId == id)
    {
        AddOrderBy(model => model.CreatedAt, true);
    }
}