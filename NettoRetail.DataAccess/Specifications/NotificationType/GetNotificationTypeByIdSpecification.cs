using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.NotificationType;

public class GetNotificationTypeByIdSpecification : Specification<NotificationTypeEntity>
{
    public GetNotificationTypeByIdSpecification(int id) : base(notificationType => notificationType.Id == id)
    {
    }
}