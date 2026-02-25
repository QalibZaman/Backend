using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Roles.Commands;

public record RoleUpdateCommand(int Id, int ShopId, UpdateRoleRequest Model) : IRequest<Unit>;

public class RoleUpdateCommandHandler(IRoleRepository roleRepository) : IRequestHandler<RoleUpdateCommand, Unit>
{
    public async Task<Unit> Handle(RoleUpdateCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetRoleByShopIdSpecification(request.Id, request.ShopId);
        var role = await roleRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Role not found.");

        var permissionsIds = request.Model.PermissionIds?.Distinct().ToArray() ?? [];

        request.Model.Adapt(role);

        role.PermissionRoles = permissionsIds.Select(permissionId => new PermissionRoleEntity
            {
                PermissionId = permissionId,
                RoleId = role.Id
            })
            .ToList();

        await roleRepository.UpdateAsync(role, cancellationToken);

        return Unit.Value;
    }
}