using Microsoft.Extensions.Caching.Memory;
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
    private const string KEY = "Permissions";
    private readonly RequestDelegate _next;

    public PermissionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IPermissionRoleRepository permissionRoleRepository, IMemoryCache cache)
    {
        if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
        {
            await _next(context);
            return;
        }

        if (!cache.TryGetValue(KEY, out IEnumerable<string>? permissions))
        {
            var roleId = context.GetRoleId();

            var specificationBuilder = new SpecificationBuilder<PermissionRoleEntity>();
            var specification = specificationBuilder.Add<GetPermissionsByRoleIdSpecification>(roleId).Build();
            var userPermissions = await permissionRoleRepository.GetPermissionsByRoleIdAsync(specification, context.RequestAborted);

            permissions = userPermissions;
            cache.Set(KEY, permissions);
        }

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
