using Microsoft.AspNetCore.Authorization;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.API.Infrastructure.Authorization;

public class MustOwnInventoryItemRequirement : IAuthorizationRequirement
{ }

public class MustOwnInventoryItemHandler(IInventoryItemRepository inventoryItemRepository)
    : AuthorizationHandler<MustOwnInventoryItemRequirement, Guid>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MustOwnInventoryItemRequirement requirement,
        Guid inventoryItemid)
    {
        var userShopId = context.User.GetUserShopId();

        var specification = new GetInventoryItemByIdSpecification(inventoryItemid);
        var inventoryItem = await inventoryItemRepository.GetAsync(specification, CancellationToken.None);

        if (inventoryItem == null || inventoryItem.ShopId != userShopId)
        {
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }
}
