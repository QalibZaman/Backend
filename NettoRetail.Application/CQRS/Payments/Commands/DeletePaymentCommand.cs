using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Services;
using NettoRetail.Shared.Interfaces.Utilities;
using System.ComponentModel.DataAnnotations;

namespace NettoRetail.Application.CQRS.Payments.Commands;

public record DeletePaymentCommand(Guid AccountId, string PaymentMethodId, int ShopId) : IRequest<PaymentMethodDeleteResponse>;

public class DeletePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IStripePaymentService stripePaymentService,
    ISubscriptionPlanRepository subscriptionPlanRepository,
    IShopRepository shopRepository,
    IUnitOfWork unitOfWork,
    IStripeService stripeService) : IRequestHandler<DeletePaymentCommand, PaymentMethodDeleteResponse>
{
    public async Task<PaymentMethodDeleteResponse> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await GetPaymentMethodAsync(request, cancellationToken);

        if (!await stripeService.PaymentMethodExistsAsync(request.PaymentMethodId, cancellationToken))
        {
            throw new NotFoundException(HandlerConstants.Payment.PaymentMethodNotFoundMessage);
        }

        var isLastMethod = payment.Account.PaymentMethods?.Count == 1;
        var fallbackMethod = await GetFallbackMethodIfNeededAsync(request, payment, cancellationToken);

        if (fallbackMethod != null)
        {
            await SetStripeFallbackAsDefaultAsync(payment, fallbackMethod, cancellationToken);
        }

        using var transaction = unitOfWork.BeginTransaction();

        if (fallbackMethod != null)
        {
            fallbackMethod.IsDefault = true;
            await paymentRepository.UpdateAsync(fallbackMethod, cancellationToken);
        }

        if (isLastMethod)
        {
            await RevertToDefaultPlanAsync(request.ShopId, cancellationToken);
        }

        await stripePaymentService.DetachPaymentMethodAsync(request.PaymentMethodId);
        await paymentRepository.DeleteAsync(payment, cancellationToken);

        transaction.Commit();

        return new PaymentMethodDeleteResponse
        {
            IsLastPaymentMethod = isLastMethod
        };
    }

    private async Task<PaymentMethodEntity> GetPaymentMethodAsync(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetPaymentMethodByIdSpecification(request.AccountId, request.PaymentMethodId);

        return await paymentRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Payment.PaymentMethodNotFoundMessage);
    }

    private async Task<PaymentMethodEntity?> GetFallbackMethodIfNeededAsync(
        DeletePaymentCommand request,
        PaymentMethodEntity payment,
        CancellationToken cancellationToken)
    {
        if (!payment.IsDefault)
        {
            return null;
        }

        return await paymentRepository.GetFallbackPaymentMethodAsync(
            request.AccountId,
            request.PaymentMethodId,
            cancellationToken);
    }

    private async Task SetStripeFallbackAsDefaultAsync(
        PaymentMethodEntity paymentToDelete,
        PaymentMethodEntity fallbackMethod,
        CancellationToken cancellationToken)
    {
        var customerId = paymentToDelete.Account.StripeCustomerId;

        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ValidationException(HandlerConstants.Stripe.StripeCustomerIsNotConfiguredMessage);
        }

        await stripePaymentService.SetDefaultPaymentMethodAsync(
            customerId,
            fallbackMethod.StripePaymentMethodId,
            cancellationToken);
    }

    private async Task RevertToDefaultPlanAsync(int shopId, CancellationToken cancellationToken)
    {
        const int DefaultPlanId = 1;
        var specification = new GetShopByIdSpecification(shopId);
        var shop = await shopRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException(string.Format(HandlerConstants.Shop.ShopNotFoundMessage, shopId));

        if (shop.SubscriptionPlanId == DefaultPlanId || string.IsNullOrEmpty(shop.StripeCustomerId))
        {
            return;
        }

        var freePlan = await subscriptionPlanRepository.GetByTypeAsync(SubscriptionTypeEnum.Free, cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Subscription.SubscriptionPlanNotFoundMessage);

        var stripeSubscriptionId = await stripeService.CreateSubscriptionAsync(shop.StripeCustomerId, freePlan.SubscriptionId, cancellationToken);

        shop.SubscriptionPlanId = DefaultPlanId;
        shop.StripeSubscriptionId = stripeSubscriptionId;
        await shopRepository.UpdateAsync(shop, cancellationToken);

    }
}
