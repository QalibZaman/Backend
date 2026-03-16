using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.SubscriptionPlan;

public class GetSubscriptionPlanByEnumSpecification(SubscriptionTypeEnum subscriptionType)
    : Specification<SubscriptionPlanEntity>(x => x.Type == subscriptionType)
{ }
