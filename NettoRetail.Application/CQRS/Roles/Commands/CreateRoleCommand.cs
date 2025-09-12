using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.DataAccess.Specifications.Permissions;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Roles.Commands;

public record CreateRoleCommand(string Name, string Description, int ShopId, int[] PermissionIds) : IRequest<CreateRoleResponse>;

public class CreateRoleCommandHandler(IPermissionRepository permissionRepository,
    IRoleRepository roleRepository,
    IShopRepository shopRepository)
    : IRequestHandler<CreateRoleCommand, CreateRoleResponse>
{
    public async Task<CreateRoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var shopExists = await shopRepository.Exists(new GetShopByIdSpecification(request.ShopId), cancellationToken);
        if (!shopExists)
        {
            throw new NotFoundException($"No shop with Id: {request.ShopId}");
        }

        var role = request.Adapt<RoleEntity>();

        var validPermissionsSpecification = new GetValidPermissionsSpecification(request.PermissionIds);

        var validPermissions = await permissionRepository.GetIdsBySpecAsync(validPermissionsSpecification, cancellationToken);

        if (request.PermissionIds.Count() > validPermissions.Count() || validPermissions.Count() == 0)
        {
            throw new BadRequestException("Some of the permissions doesn't exist, please provide right permissions");
        }

        var permissionRoles = request.PermissionIds.Select(value => new PermissionRoleEntity() { PermissionId = value, RoleId = role.Id }).ToList();

        role.PermissionRoles = permissionRoles;

        var result = await roleRepository.AddAsync(role, cancellationToken);

        return result.Adapt<CreateRoleResponse>();
    }
}
