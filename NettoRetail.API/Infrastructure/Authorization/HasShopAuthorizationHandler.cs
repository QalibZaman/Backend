using Microsoft.AspNetCore.Authorization;

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
        if (context.User.HasClaim(c => c.Type == "shop_id"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
