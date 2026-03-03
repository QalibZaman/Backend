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
        var shopExists = await shopRepository.Exists(
            new GetShopByIdSpecification(request.ShopId),
            cancellationToken);

        if (!shopExists)
            throw new NotFoundException($"No shop with Id: {request.ShopId}");

        var roleSpec = new GetRoleByShopIdSpecification(request.Id, request.ShopId);
        roleSpec.AddInclude(r => r.PermissionRoles);

        var role = await roleRepository.GetAsync(roleSpec, cancellationToken)
            ?? throw new NotFoundException("The role with such id doesn't exist.");

        if (await roleRepository.Exists(
                new GetRoleByNameExcludingCurrentSpecification(request.Model.Name, request.Id),
                cancellationToken))
        {
            throw new ConflictException("Role with such name already exists");
        }

        var permissionIds = request.Model.PermissionIds?.Distinct().ToArray() ?? Array.Empty<int>();
        if (permissionIds.Length == 0)
            throw new BadRequestException("At least one permission must be selected.");

        if (permissionIds.Any(id => id <= 0))
            throw new BadRequestException("PermissionId must be greater than 0.");

        var validPermissionIds = (await permissionRepository.GetIdsBySpecAsync(
                new GetValidPermissionsSpecification(permissionIds),
                cancellationToken))
            .ToHashSet();

        if (permissionIds.Any(id => !validPermissionIds.Contains(id)))
            throw new BadRequestException("Some permissions don't exist. Please provide valid permission ids.");

        role.Name = request.Model.Name;
        role.Description = request.Model.Description;

        role.PermissionRoles ??= new List<PermissionRoleEntity>();

        var toRemove = role.PermissionRoles
            .Where(pr => !validPermissionIds.Contains(pr.PermissionId))
            .ToList();

        foreach (var pr in toRemove)
            role.PermissionRoles.Remove(pr);

        var existing = role.PermissionRoles.Select(pr => pr.PermissionId).ToHashSet();
        foreach (var pid in validPermissionIds)
        {
            if (existing.Contains(pid)) continue;

            role.PermissionRoles.Add(new PermissionRoleEntity
            {
                RoleId = role.Id,
                PermissionId = pid
            });
        }

        await roleRepository.UpdateAsync(role, cancellationToken);
    }
}