using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.DataAccess.Specifications.SubscriptionPlan;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Subscriptions.Commands;

public record SubscribeToThePlanCommand(
    int ShopId,
    Guid UserId,
    SubscribeRequest Model) : IRequest;

public class SubscribeToThePlanCommandHandler(
    IStaffRepository staffRepository,
    IShopRepository shopRepository,
    IPaymentRepository paymentRepository,
    IStripePaymentService stripePaymentService,
    ISubscriptionPlanRepository subscriptionPlanRepository,
    ISubscriptionRepository subscriptionRepository) : IRequestHandler<SubscribeToThePlanCommand>
{
    public async Task Handle(SubscribeToThePlanCommand request, CancellationToken cancellationToken)
    {
        var account =
            await staffRepository.GetAsync(
                new GetStaffByIdSpecification(request.UserId),
                cancellationToken)
            ?? throw new NotFoundException(
                string.Format(
                    HandlerConstants.Payment.AccountNotFoundMessage,
                    request.UserId));

        var shop =
            await shopRepository.GetAsync(
                new GetShopByIdSpecification(request.ShopId),
                cancellationToken)
            ?? throw new NotFoundException(
                string.Format(
                    HandlerConstants.Payment.ShopNotFoundMessage,
                    request.ShopId));

        if (account.ShopId != request.ShopId)
        {
            throw new ForbiddenException("Subscription Not Allowed");
        }

        var paymentMethod =
            await paymentRepository.GetAsync(
                new GetPaymentMethodByStripeIdAndAccountId(
                    request.Model.PaymentMethodId,
                    request.UserId),
                cancellationToken)
            ?? throw new NotFoundException("Payment Method Not Found");

        var customerId = await stripePaymentService.EnsureCustomerAsync(
            account.StripeCustomerId,
            $"{account.FirstName} {account.LastName}",
            account.Email,
            cancellationToken);

        var stripePaymentMethodId = paymentMethod.StripePaymentMethodId;
        var plan = await subscriptionPlanRepository.GetAsync(
                       new GetSubscriptionPlanByIdSpecification(request.Model.SubscriptionPlanId), cancellationToken) ??
                   throw new NotFoundException("Subscription Plan Not Found");

        if (!plan.IsActive)
        {
            throw new ConflictException("Subscription plan is not active.");
        }

        var subscription = await subscriptionRepository.GetActiveByShopIdAsync(shop.Id, cancellationToken);
        if (subscription is not null)
        {
            throw new ConflictException("Shop already has an active subscription.");
        }

        var stripePriceId = plan.StripePriceId;

        var stripeSubscriptionId =
            await stripePaymentService.CreateSubscriptionAsync(
                customerId,
                stripePaymentMethodId,
                stripePriceId,
                cancellationToken);

        var subscriptionEntity = new SubscriptionEntity
        {
            AccountId = account.Id,
            ShopId = shop.Id,
            SubscriptionPlanId = plan.Id,
            PaymentMethodId = paymentMethod.Id,
            StripeSubscriptionId = stripeSubscriptionId,
            Status = SubscriptionStatusEnum.Active,
            StartedAt = DateTime.UtcNow
        };
        await subscriptionRepository.AddAsync(
            subscriptionEntity,
            cancellationToken);
    }
}