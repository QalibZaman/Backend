using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Subscriptions;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Subscription.Queries;
public record GetSubscriptionsPlansQuery(int ShopId) : IRequest<List<SubscriptionPlanResponse>>;
public class GetSubscriptionsPlansQueryHandler(
    ISubscriptionPlanRepository subscriptionPlanRepository,IShopRepository shopRepository) : IRequestHandler<GetSubscriptionsPlansQuery, List<SubscriptionPlanResponse>>
{
    public async Task<List<SubscriptionPlanResponse>> Handle(GetSubscriptionsPlansQuery request, CancellationToken cancellationToken)
    {
        var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken)
            ?? throw new NotFoundException("Shop not found");

        var plans = await subscriptionPlanRepository.GetAllAsync(cancellationToken);

        return plans.Select(s =>
        {
            var response = s.Adapt<SubscriptionPlanResponse>();
            response.IsActive = s.Id == shop.SubscriptionPlanId;
            return response;
        }).ToList();
    }
}
