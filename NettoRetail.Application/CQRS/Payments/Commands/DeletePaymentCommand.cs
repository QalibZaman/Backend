using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.DataAccess.Specifications.Subscription;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Payments.Commands;

public sealed record DeletePaymentCommand(Guid AccountId, int ShopId, string PaymentMethodId)
    : IRequest<PaymentMethodDeleteResponse>;

public sealed class DeletePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IShopRepository shopRepository,
    IStripePaymentService stripePaymentService,
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeletePaymentCommand, PaymentMethodDeleteResponse>
{
    private const int FreePlanId = 1;

    public async Task<PaymentMethodDeleteResponse> Handle(
        DeletePaymentCommand request,
        CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetAsync(
                          new GetPaymentMethodByIdSpecification(request.AccountId, request.PaymentMethodId),
                          cancellationToken)
                      ?? throw new NotFoundException(HandlerConstants.Payment.PaymentMethodNotFoundMessage);

        var shop = await shopRepository.GetAsync(
                       new GetShopByIdSpecification(request.ShopId), cancellationToken)
                   ?? throw new NotFoundException(string.Format(
                       HandlerConstants.Payment.ShopNotFoundMessage,
                       request.ShopId));
        var subscriptionEntity = await subscriptionRepository.GetAsync(
            new GetSubscriptionByShopIdSpecification(shop.Id),
            cancellationToken);
        var renewingSubscription =
            await GetRenewingSubscriptionAsync(payment, shop, subscriptionEntity, cancellationToken);

        using var transaction = unitOfWork.BeginTransaction();

        await paymentRepository.DeleteAsync(payment, cancellationToken);

        if (renewingSubscription is not null)
        {
            await stripePaymentService.SetSubscriptionCancelAtPeriodEndAsync(shop.StripeSubscriptionId!, true,
                cancellationToken);
        }

        try
        {
            await stripePaymentService.DetachPaymentMethodAsync(payment.StripePaymentMethodId, cancellationToken);
        }
        catch
        {
            if (renewingSubscription is not null)
            {
                await TryRestoreAutoRenewalAsync(shop.StripeSubscriptionId!);
            }

            throw;
        }

        transaction.Commit();

        return new PaymentMethodDeleteResponse
        {
            IsAutoRenewalDisabled = renewingSubscription is not null,
            SubscriptionExpiresAt = renewingSubscription?.CurrentPeriodEnd
        };
    }

    // returns the subscription only if this card is what Stripe will charge for the next renewal
    private async Task<SubscriptionSnapshot?> GetRenewingSubscriptionAsync(
        PaymentMethodEntity payment,
        ShopEntity shop,
        SubscriptionEntity subscriptionEntity,
        CancellationToken cancellationToken)
    {
        if (!payment.IsDefault
            || subscriptionEntity.SubscriptionPlanId == FreePlanId
            || string.IsNullOrEmpty(shop.StripeSubscriptionId)
            || !string.Equals(payment.Account.StripeCustomerId, shop.StripeCustomerId, StringComparison.Ordinal))
        {
            return null;
        }

        var subscription =
            await stripePaymentService.GetSubscriptionAsync(shop.StripeSubscriptionId, cancellationToken);

        return subscription.IsRenewing ? subscription : null;
    }

    private async Task TryRestoreAutoRenewalAsync(string subscriptionId)
    {
        try
        {
            await stripePaymentService.SetSubscriptionCancelAtPeriodEndAsync(subscriptionId, false,
                CancellationToken.None);
        }
        catch (PaymentExceptions)
        {
            // already logged by the Stripe service; the caller should see the original failure
        }
    }
}