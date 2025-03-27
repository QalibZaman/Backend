using Microsoft.AspNetCore.Authorization;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Shared.Constants;

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
        if (context.User.HasClaim(c => c.Type == AuthClaims.ShopId))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
