using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Response.Roles;
using NettoRetail.DataAccess.Specifications.Permissions;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Roles.Commands;

public record CreateRoleCommand(
    CreateRoleRequest Model,
    int ShopId) : IRequest<CreateRoleResponse>;

public class CreateRoleCommandHandler(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IShopRepository shopRepository) : IRequestHandler<CreateRoleCommand, CreateRoleResponse>
{
    public async Task<CreateRoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var shopExists = await shopRepository.Exists(
            new GetShopByIdSpecification(request.ShopId),
            cancellationToken);

        if (!shopExists)
            throw new NotFoundException($"No shop with Id: {request.ShopId}");

        if (request.Model.PermissionIds is null || request.Model.PermissionIds.Length == 0)
            throw new BadRequestException("At least one permission must be enabled.");

        var distinctPermissionIds = request.Model.PermissionIds.Distinct().ToArray();
        var validSpec = new GetValidPermissionsSpecification(distinctPermissionIds);
        var validPermissions = await permissionRepository.GetIdsBySpecAsync(validSpec, cancellationToken);

        if (validPermissions.Count() != distinctPermissionIds.Length)
            throw new BadRequestException("Some of the permissions don't exist. Please provide valid permissions.");

        if (await roleRepository.Exists(new GetRoleByNameAndShopIdSpecification(request.Model.Name, request.ShopId), cancellationToken))
            throw new ConflictException("Role with such name already exists in your shop");

        var role = new RoleEntity
        {
            Name = request.Model.Name.Trim(),
            Description = !string.IsNullOrWhiteSpace(request.Model.Description) ? request.Model.Description?.Trim() : null,
            ShopId = request.ShopId
        };

        role.PermissionRoles = distinctPermissionIds
            .Select(pid => new PermissionRoleEntity
            {
                PermissionId = pid,
                Role = role
            })
            .ToList();

        var created = await roleRepository.AddAsync(role, cancellationToken);

        return new CreateRoleResponse
        {
            Id = created.Id,
            Name = created.Name,
            Description = created.Description,
            ShopId = created.ShopId,
            PermissionIds = role.PermissionRoles.Select(x => x.PermissionId).ToList()
        };
    }
}
