using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Services;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Subscriptions.Commands;

public record ChangeSubscriptionCommand(
    int ShopId,
    Guid UserId,
    SubscriptionTypeEnum NewPlan,
    string PaymentMethodId) : IRequest;

public class ChangeSubscriptionCommandHandler(
    IShopRepository shopRepository,
    IAccountRepository accountRepository,
    ISubscriptionPlanRepository subscriptionPlanRepository,
    IShopSubscriptionService subscriptionService) : IRequestHandler<ChangeSubscriptionCommand>
{
    public async Task Handle(ChangeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var shopSpec = new GetShopByIdSpecification(request.ShopId);
        var shop = await shopRepository.GetAsync(shopSpec, cancellationToken)
            ?? throw new NotFoundException($"Shop with ID {request.ShopId} not found.");

        var adminSpec = new GetAccountByShopIdAndRoleSpecification(request.ShopId, RoleConstants.Admin);
        var admin = await accountRepository.GetAsync(adminSpec, cancellationToken)
            ?? throw new ForbiddenException("Shop has no admin.");

        if (admin.Id != request.UserId)
            throw new ForbiddenException("You are not the owner of this shop");

        var plan = await subscriptionPlanRepository
            .GetByTypeAsync(request.NewPlan, cancellationToken)
            ?? throw new InvalidOperationException("Subscription plan not found");

        if (shop.SubscriptionPlanId == plan.Id)
            throw new ConflictException("Shop already has this subscription plan.");

        await subscriptionService.ChangeSubscriptionAsync(
            shop,
            request.NewPlan,
            request.PaymentMethodId,
            cancellationToken);
    }
}