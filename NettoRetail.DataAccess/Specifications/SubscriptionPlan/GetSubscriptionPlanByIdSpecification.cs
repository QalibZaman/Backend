using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.SubscriptionPlan;

public class GetSubscriptionPlanByIdSpecification : Specification<SubscriptionPlanEntity>
{
    public GetSubscriptionPlanByIdSpecification(int id) : base(plan => plan.Id == id)
    {
    }
}