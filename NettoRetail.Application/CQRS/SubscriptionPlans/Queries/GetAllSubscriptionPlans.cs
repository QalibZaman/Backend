using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.DataAccess.Specifications.Subscription;
using NettoRetail.Shared.Interfaces.Repositories;
using Stripe;

namespace NettoRetail.Application.CQRS.SubscriptionPlans.Queries
{
    public record GetSubscriptionsPlansQuery(int shopId) : IRequest<List<SubscriptionPlanResponce>>;

    public class GetAllSubscriptionPlans(
        ISubscriptionPlanRepository subscriptionPlanRepository,
        IShopRepository shopRepository,
        IMapper mapper,
        ISubscriptionRepository subscriptionRepository)
        : IRequestHandler<GetSubscriptionsPlansQuery, List<SubscriptionPlanResponce>>
    {
        public async Task<List<SubscriptionPlanResponce>> Handle(GetSubscriptionsPlansQuery request,
            CancellationToken cancellationToken)
        {
            var shop = await shopRepository.GetAsync(
                           new GetShopByIdSpecification(request.shopId).AddInclude(shop => shop.Account),
                           cancellationToken)
                       ?? throw new NotFoundException("Shop not found");
            var subscription = await subscriptionRepository.GetAsync(new GetSubscriptionByShopIdSpecification(shop.Id),
                cancellationToken);
            var items = await subscriptionPlanRepository.GetAllAsync(cancellationToken);
            return items.Select(i =>
            {
                var responce = mapper.Map<SubscriptionPlanResponce>(i);
                responce.IsActive = i.Id == subscription.SubscriptionPlanId;
                return responce;
            }).ToList();
        }
    }
}