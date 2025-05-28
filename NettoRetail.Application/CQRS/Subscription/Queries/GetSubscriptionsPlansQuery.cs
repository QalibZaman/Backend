using Mapster;
using MediatR;
using NettoRetail.Application.Models.Subscriptions;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Subscription.Queries;
public record GetSubscriptionsPlansQuery() : IRequest<List<SubscriptionPlanResponse>>;
public class GetSubscriptionsPlansQueryHandler(ISubcriptionPlanRepository subscriptionPlanRepository) : IRequestHandler<GetSubscriptionsPlansQuery, List<SubscriptionPlanResponse>>
{
    public async Task<List<SubscriptionPlanResponse>> Handle(GetSubscriptionsPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await subscriptionPlanRepository.ListAllAsync(cancellationToken);

        return plans.Adapt<List<SubscriptionPlanResponse>>();
    }
}