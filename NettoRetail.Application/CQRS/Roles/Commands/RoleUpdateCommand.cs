using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.DataAccess.Specifications.Permissions;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Roles.Commands;

public record RoleUpdateCommand(int Id, int ShopId, UpdateRoleRequest Model) : IRequest;

public class RoleUpdateCommandHandler(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IShopRepository shopRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RoleUpdateCommand>
{
    public async Task Handle(RoleUpdateCommand request, CancellationToken cancellationToken)
    {
        var shopExists = await shopRepository.Exists(new GetShopByIdSpecification(request.ShopId), cancellationToken);
        if (!shopExists) throw new NotFoundException($"No shop with Id: {request.ShopId}");

        var roleByIdSpecification = new GetRoleByShopIdSpecification(request.Id, request.ShopId);
        var role = await roleRepository.GetAsync(roleByIdSpecification, cancellationToken) ?? throw new NotFoundException("The role with such id doesn't exist.");
        request.Model.Adapt(role);
        var permissionsIds = request.Model.PermissionIds?.Distinct().ToArray()
                               ?? Array.Empty<int>();
        if (permissionsIds.Length == 0)
        {
            throw new BadRequestException("At least one permission must be selected.");
        }

        var validPermissions = await permissionRepository.GetIdsBySpecAsync(
            new GetValidPermissionsSpecification(permissionsIds), cancellationToken);

        if ( permissionsIds.Length != validPermissions.Count())
        {
            throw new BadRequestException("Some of the permissions doesn't exist, please provide right permissions");
        }

        var permissionRoles = permissionsIds.Select(value => new PermissionRoleEntity { PermissionId = value, RoleId = role.Id }).ToList();
        role.PermissionRoles = permissionRoles;
        await roleRepository.UpdateAsync(role, cancellationToken);
    }
}