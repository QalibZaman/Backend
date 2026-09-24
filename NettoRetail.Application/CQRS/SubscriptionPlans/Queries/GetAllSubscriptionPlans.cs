using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Interfaces.Repositories;
using Stripe;

namespace NettoRetail.Application.CQRS.SubscriptionPlans.Queries
{
    public record GetSubscriptionsPlansQuery(int shopId) : IRequest<List<SubscriptionPlanResponce>>;
    public class GetAllSubscriptionPlans(
        ISubscriptionPlanRepository subscriptionPlanRepository,
        IShopRepository shopRepository,
        IMapper mapper)
        : IRequestHandler<GetSubscriptionsPlansQuery, List<SubscriptionPlanResponce>>
    {
        public async Task<List<SubscriptionPlanResponce>> Handle(GetSubscriptionsPlansQuery request, CancellationToken cancellationToken)
        {
            var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.shopId), cancellationToken)
            ?? throw new NotFoundException("Shop not found");
            var items = await subscriptionPlanRepository.GetAllAsync(cancellationToken);
            return items.Select(i =>
            {
                var responce = mapper.Map<SubscriptionPlanResponce>(i);
                responce.IsActive = i.Id == shop.SubscriptionPlanId;
                return responce;
            }).ToList();
        }
    }
}
