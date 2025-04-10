using NettoRetail.API.Infrastructure.Attributes;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.PermissionRole;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.API.Infrastructure.Middlewares;

public class PermissionMiddleware
{
    private readonly RequestDelegate _next;

    public PermissionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IPermissionRoleRepository permissionRoleRepository)
    {
        if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
        {
            await _next(context);
            return;
        }

        var roleId = context.GetRoleId();

        var specificationBuilder = new SpecificationBuilder<PermissionRoleEntity>();
        var specification = specificationBuilder.Add<GetPermissionsByRoleIdSpecification>(roleId).Build();
        var permissions = await permissionRoleRepository.GetPermissionsByRoleIdAsync(specification, context.RequestAborted);

        var endpoint = context.GetEndpoint();
        var requiredPermissions = endpoint?.Metadata?.GetOrderedMetadata<RequiresPermissionAttribute>();

        if (requiredPermissions != null && requiredPermissions.Any())
        {
            var missingPermissions = requiredPermissions
                .Where(required => !permissions.Any(permission => permission == required.Permission))
                .Select(rp => rp.Permission)
                .ToList();

            if (missingPermissions.Any())
            {
                throw new ForbiddenException("You don't have required permissions");
            }
        }

        await _next(context);
        return;
    }
}
