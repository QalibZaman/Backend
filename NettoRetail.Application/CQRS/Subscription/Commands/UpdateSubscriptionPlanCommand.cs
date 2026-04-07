using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.DataAccess.Specifications.SubscriptionPlan;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Subscriptions.Commands;

public record UpdateSubscriptionPlanCommand(
    int PlanId,
    Guid UserId,
    UpdateSubscriptionPlanRequest SubscriptionPlanRequest) : IRequest;

public class UpdateSubscriptionPlanCommandHandler(
    ISubscriptionPlanRepository planRepository,
    IStaffRepository accountRepository,
    IFeatureRepository featureRepository,
    IPriceHistoryService priceHistoryService,
    ISubscriptionNotificationService subscriptionNotificationService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateSubscriptionPlanCommand>
{
    public async Task Handle(UpdateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var superAdmin = await accountRepository.GetAsync(
            new GetStaffByIdSpecification(request.UserId), cancellationToken)
            ?? throw new NotFoundException(string.Format(HandlerConstants.Account.AccountNotFoundMessage, request.UserId));

        var specification = new GetSubscriptionPlanByIdSpecification(request.PlanId);

        var plan = await planRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Subscription.SubscriptionPlanNotFoundMessage);

        var newFeatures = await featureRepository.GetAllByIdsAsync(request.SubscriptionPlanRequest.FeatureIds!, cancellationToken);
        plan.Features = newFeatures.ToList();

        if (!string.Equals(plan.Name, request.SubscriptionPlanRequest.Name, StringComparison.Ordinal))
            plan.Name = request.SubscriptionPlanRequest.Name;

        if (!string.Equals(plan.Description, request.SubscriptionPlanRequest.Description, StringComparison.Ordinal))
            plan.Description = request.SubscriptionPlanRequest.Description!;

        if (!string.Equals(plan.Moto, request.SubscriptionPlanRequest.Motto, StringComparison.Ordinal))
            plan.Moto = request.SubscriptionPlanRequest.Motto!;

        var currentPrice = await priceHistoryService.GetCurrentPriceAsync(plan.Id, cancellationToken);

        using var transaction = unitOfWork.BeginTransaction();

        if (currentPrice != request.SubscriptionPlanRequest.Price)
        {
            var effectiveDate = await priceHistoryService.CalculateEffectiveDateAsync(DateTime.UtcNow);

            await priceHistoryService.UpdatePlanPriceAsync(plan.Id, request.SubscriptionPlanRequest.Price, effectiveDate, cancellationToken);
            await subscriptionNotificationService.NotifyPriceChangedAsync(plan, currentPrice, request.SubscriptionPlanRequest.Price, effectiveDate, cancellationToken);
        }

        await planRepository.UpdateAsync(plan, cancellationToken);

        transaction.Commit();
    }
}