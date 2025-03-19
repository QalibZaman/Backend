using Microsoft.AspNetCore.Authorization;
using NettoRetail.API.Infrastructure.Extensions;

namespace NettoRetail.API.Infrastructure.Authorization;

public class HasShopAuthorizationRequirement : IAuthorizationRequirement
{ }

public class HasShopAuthorizationHandler
    : AuthorizationHandler<HasShopAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HasShopAuthorizationRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == MyClaimTypes.ShopId))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
