using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Notification;

public class GetNotificationsByStaffIdSpecificationWithPagination: Specification<NotificationEntity>
{
    public GetNotificationsByStaffIdSpecificationWithPagination(Guid id, int skip, int take)
        : base(model => model.StaffId == id)
    {
        Skip = skip;
        Take = take;
        AddOrderBy(model => model.CreatedAt, true);
    }
}