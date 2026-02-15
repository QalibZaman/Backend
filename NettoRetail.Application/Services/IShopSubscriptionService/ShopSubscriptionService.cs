using NettoRetail.Application.Exceptions;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Services;
using Stripe;

namespace NettoRetail.Application.Services;

public class ShopSubscriptionService(
    ISubscriptionPlanRepository subscriptionPlanRepository,
    IShopRepository shopRepository,
    IStripeService stripeService
) : IShopSubscriptionService
{
    public async Task ChangeSubscriptionAsync(
        ShopEntity shop,
        SubscriptionTypeEnum newPlan,
        string paymentMethodId,
        CancellationToken cancellationToken)
    {
        var exist = await stripeService.PaymentMethodExistsAsync(paymentMethodId, cancellationToken);
        if (!exist)
        {
            throw new BadRequestException("Payment method does not exist.Subscription is not updated.");
        }

        var plan = await subscriptionPlanRepository
            .GetByTypeAsync(newPlan, cancellationToken)
            ?? throw new NotFoundException("Subscription plan not found.Subscription is not updated.");

        if (string.IsNullOrWhiteSpace(shop.StripeCustomerId))
        {
            try
            {
                shop.StripeCustomerId = await stripeService.CreateCustomerAsync(
                    shop.Email,
                    paymentMethodId,
                    cancellationToken);
            }
            catch (StripeException e)
            {
                var msg = e.StripeError?.Message ?? e.Message;
                var code = e.StripeError?.Code;
                throw new BadRequestException($"Stripe error ({code}): {msg}");
            }
        }

        var subscriptionId = await stripeService.CreateSubscriptionAsync(
            shop.StripeCustomerId,
            plan.SubscriptionId,
            cancellationToken);

        shop.SubscriptionPlanId = plan.Id;
        shop.StripeSubscriptionId = subscriptionId;

        shop = await shopRepository.UpdateAsync(shop, cancellationToken);
    }
}