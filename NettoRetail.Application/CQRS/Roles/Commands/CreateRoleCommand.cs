using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Azure.Cosmos;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Response.Roles;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;

namespace NettoRetail.Application.CQRS.Roles.Commands;

public record CreateRoleCommand(
 CreateRoleRequest Model,
 int ShopId) : IRequest<CreateRoleResponse>;

public class CreateRoleCommandHandler(
    IRoleRepository roleRepository,
    IMapper mapper) : IRequestHandler<CreateRoleCommand, CreateRoleResponse>
{
    public async Task<CreateRoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = new RoleEntity
        {
            Name = request.Model.Name.Trim(),
            Description = !string.IsNullOrWhiteSpace(request.Model.Description) ? request.Model.Description?.Trim() : null,
            ShopId = request.ShopId,
            PermissionRoles = []
        };

        foreach (var permission in request.Model.Permissions)
        {
            var per = new PermissionRoleEntity
            {
                PermissionId = permission.PermissionId,
                AccessLevel = permission.AccessLevel
            };
            role.PermissionRoles.Add(per);
        }

        var newRole = await roleRepository.AddAsync(role, cancellationToken);

        var roleResponse = mapper.Map<CreateRoleResponse>(newRole);

        return roleResponse;
    }
}