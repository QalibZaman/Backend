using MediatR;
using NettoRetail.Domain.Entities;
using NettoRetail.Application.Exceptions;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Constants;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Subscriptions.Commands;

public record UpdateSubscriptionPlanCommand(
    int PlanId,
    Guid UserId,
    string Name,
    int Price,
    string? Description,
    string? Motto,
    List<int> FeatureIds
) : IRequest;

public class UpdateSubscriptionPlanCommandHandler(
    ISubscriptionPlanRepository planRepository,
    IStaffRepository accountRepository,
    IFeatureRepository featureRepository,
    IPriceHistoryService priceHistoryService,
    ISubscriptionNotificationService subscriptionNotificationService
) : IRequestHandler<UpdateSubscriptionPlanCommand>
{
    public async Task Handle(UpdateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var superAdmin = await accountRepository.GetAsync(
            new GetStaffByIdSpecification(request.UserId), cancellationToken)
            ?? throw new NotFoundException("This account was not found");

        if (superAdmin.RoleId != RoleConstants.SuperAdmin)
            throw new ForbiddenException("Only SuperAdmin can update subscription plans");

        if (request.FeatureIds == null || request.FeatureIds.Count == 0)
            throw new InvalidDataException("At least one feature must be enabled for a subscription plan");

        var specification = new SpecificationBuilder<SubscriptionPlanEntity>()
            .AddCriteria(plan => plan.Id == request.PlanId)
            .AddInclude(p => p.Shops)
            .AddInclude(p => p.Features)
            .Build();

        var plan = await planRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException("Subscription plan not found");

        var newFeatures = await featureRepository.GetAllByIdsAsync(request.FeatureIds, cancellationToken);
        plan.Features = newFeatures.ToList();

        if (!string.Equals(plan.Name, request.Name, StringComparison.Ordinal))
            plan.Name = request.Name;

        if (!string.Equals(plan.Description, request.Description, StringComparison.Ordinal))
            plan.Description = request.Description;

        if (!string.Equals(plan.Moto, request.Motto, StringComparison.Ordinal))
            plan.Moto = request.Motto;

        var currentPrice = await priceHistoryService.GetCurrentPriceAsync(plan.Id, cancellationToken);

        if (currentPrice != request.Price)
        {
            var effectiveDate = await priceHistoryService.CalculateEffectiveDateAsync(DateTime.UtcNow);

            await priceHistoryService.UpdatePlanPriceAsync(plan.Id, request.Price, effectiveDate, cancellationToken);

            await subscriptionNotificationService.NotifyPriceChangedAsync(plan, currentPrice, request.Price, effectiveDate, cancellationToken);
        }

        await planRepository.UpdateAsync(plan, cancellationToken);
    }
}