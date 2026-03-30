using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Response.Roles;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Roles.Commands;

public record CreateRoleCommand(
    CreateRoleRequest Model,
    int ShopId) : IRequest<CreateRoleResponse>;

public class CreateRoleCommandHandler(
    IRoleRepository roleRepository) : IRequestHandler<CreateRoleCommand, CreateRoleResponse>
{
    public async Task<CreateRoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = new RoleEntity
        {
            Name = request.Model.Name.Trim(),
            Description = !string.IsNullOrWhiteSpace(request.Model.Description) ? request.Model.Description?.Trim() : null,
            ShopId = request.ShopId
        };

        role.PermissionRoles = request.Model.PermissionIds
            .Distinct()
            .Select(pid => new PermissionRoleEntity
            {
                PermissionId = pid,
                Role = role
            })
            .ToList();

        var newRole = await roleRepository.AddAsync(role, cancellationToken);

        var roleResponse = newRole.Adapt<CreateRoleResponse>();
        return roleResponse;
    }
}
