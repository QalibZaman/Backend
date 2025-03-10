using Microsoft.AspNetCore.Authorization;

namespace NettoRetail.API.Infrastructure.Authorization;

public class ShopAuthorizationRequirement : IAuthorizationRequirement
{ }

public class ShopAuthorizationHandler
    : AuthorizationHandler<ShopAuthorizationRequirement, int>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ShopAuthorizationRequirement requirement,
        int shopId)
    {
        if (int.TryParse(context.User.FindFirst("shop_id")?.Value, out int userShopId) && userShopId == shopId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
