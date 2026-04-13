using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Services.IShopSubscriptionService;

public interface IShopSubscriptionService
{
    Task ChangeSubscriptionAsync(
        ShopEntity shop,
        SubscriptionTypeEnum newPlan,
        string stripePaymentMethodId,
        CancellationToken cancellationToken = default);
}