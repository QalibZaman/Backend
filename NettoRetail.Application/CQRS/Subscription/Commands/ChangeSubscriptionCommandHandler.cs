using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Services;

namespace NettoRetail.Application.CQRS.Subscriptions.Commands;

public record ChangeSubscriptionCommand(
    int shopId,
    Guid userId,
    SubscriptionTypeEnum newPlan,
    string? PaymentMethodId) : IRequest;

public class ChangeSubscriptionCommandHandler(
    IShopRepository shopRepository,
    IAccountRepository accountRepository,
    IShopSubscriptionService subscriptionService) : IRequestHandler<ChangeSubscriptionCommand>
{
    public async Task Handle(ChangeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var adminSpec = new GetAccountByShopIdAndRoleSpecification(request.shopId, RoleConstants.Admin);
        var admin = await accountRepository.GetAsync(adminSpec, cancellationToken)
            ?? throw new ForbiddenException("Shop has no admin.");

        if (admin.Id != request.userId)
            throw new ForbiddenException("You are not the owner of this shop");

        var shopSpec = new GetShopByIdSpecification(request.shopId);
        var shop = await shopRepository.GetAsync(shopSpec, cancellationToken)
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