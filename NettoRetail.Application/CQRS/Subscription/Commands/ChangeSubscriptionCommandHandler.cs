using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Services;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Subscriptions.Commands;

public record ChangeSubscriptionCommand(
    int shopId,
    SubscriptionTypeEnum newPlan,
    string? PaymentMethodId
) : IRequest;

public class ChangeSubscriptionCommandHandler(
    IShopRepository shopRepository,
    ISubcriptionPlanRepository subcriptionPlanRepository,
    IShopSubscriptionService subscriptionService
) : IRequestHandler<ChangeSubscriptionCommand>
{
    public async Task Handle(ChangeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetShopByIdSpecification(request.shopId);
        var shop = await shopRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Shop with that Id not found");

        if (shop.Subscription == request.newPlan)
            throw new ConflictException("Shop already has this subscription plan.");

        await subscriptionService.ChangeSubscriptionAsync(
            shop,
            request.newPlan,
            request.PaymentMethodId,
            cancellationToken);

    }
}