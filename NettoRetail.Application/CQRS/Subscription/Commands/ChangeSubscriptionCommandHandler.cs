using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Subscription.Commands;

public record ChangeSubscriptionCommand(
    int ShopId,
    Guid UserId,
    SubscriptionTypeEnum NewPlan,
    string PaymentMethodId) : IRequest<Unit>;

public class ChangeSubscriptionCommandHandler(
    IShopRepository shopRepository,
    IShopSubscriptionService subscriptionService) : IRequestHandler<ChangeSubscriptionCommand, Unit>
{
    public async Task<Unit> Handle(ChangeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var shopSpec = new GetShopByIdSpecification(request.ShopId);
        var shop = await shopRepository.GetAsync(shopSpec, cancellationToken)
            ?? throw new NotFoundException($"Shop with ID {request.ShopId} not found.");

        await subscriptionService.ChangeSubscriptionAsync(
            shop,
            request.NewPlan,
            request.PaymentMethodId,
            cancellationToken);

        return Unit.Value;
    }
}