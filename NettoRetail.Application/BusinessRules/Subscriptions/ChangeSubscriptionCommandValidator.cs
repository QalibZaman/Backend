using FluentValidation;
using NettoRetail.Application.CQRS.Subscription.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.DataAccess.Specifications.SubscriptionPlan;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.BusinessRules.Subscriptions;

public class ChangeSubscriptionCommandValidator : AbstractValidator<ChangeSubscriptionCommand>
{
    public ChangeSubscriptionCommandValidator(
        ISubscriptionPlanRepository subscriptionPlanRepository,
        IStaffRepository staffRepository,
        IShopRepository shopRepository)
    {
        RuleFor(x => x)
            .CustomAsync(async (command, context, ct) =>
            {
                var plan = await subscriptionPlanRepository
                    .GetAsync(new GetSubscriptionPlanByEnumSpecification(command.NewPlan), ct)
                        ?? throw new InvalidOperationException("Subscription plan not found");

                var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(command.ShopId), ct)
                    ?? throw new NotFoundException($"Shop with ID {command.ShopId} not found.");

                if (shop.SubscriptionPlanId == plan.Id) throw new ConflictException("Shop already has this subscription plan.");
            });

        RuleFor(x => x)
            .CustomAsync(async (command, context, ct) =>
            {
                var admin = await staffRepository.GetAsync(new GetStaffByIdSpecification(command.UserId), ct)
                    ?? throw new NotFoundException("Shop admin with such Id dont exists");

                if (admin!.ShopId != command.ShopId) throw new ForbiddenException("You cannot change the subsciption plan of a shop you do not belog to");
            });
    }
}
