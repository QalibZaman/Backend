using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;
using System.Linq.Expressions;

namespace NettoRetail.DataAccess.Specifications.SubscriptionPlan;

public class GetSubscriptionPlanByIdSpecification : Specification<SubscriptionPlanEntity>
{
    public GetSubscriptionPlanByIdSpecification(int planId)
        : base(x => x.Id == planId)
    {
        AddInclude(x => x.Shops);
        AddInclude(x => x.Features);
    }
}
