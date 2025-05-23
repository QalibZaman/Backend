using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Shared.Interfaces.Services;

public interface IShopSubscriptionService
{
    Task ChangeSubscriptionAsync(
        ShopEntity shop,
        SubscriptionTypeEnum newPlan,
        string? paymentMethodId,
        CancellationToken cancellationToken = default);
}