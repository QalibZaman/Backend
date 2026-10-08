using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Subscription;

public class GetSubscriptionByShopIdSpecification : Specification<SubscriptionEntity>
{
    public GetSubscriptionByShopIdSpecification(int shopId)
        : base(s => s.ShopId == shopId)
    {
    }
}