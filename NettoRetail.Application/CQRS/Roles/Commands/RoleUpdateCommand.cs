using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Roles.Commands;

public record RoleUpdateCommand(int Id, int ShopId, UpdateRoleRequest Model) : IRequest;

public class RoleUpdateCommandHandler(IRoleRepository roleRepository): IRequestHandler<RoleUpdateCommand>
{
    public async Task Handle(RoleUpdateCommand request, CancellationToken cancellationToken)
    {
        var roleByIdSpecification = new GetRoleByIdSpecification(request.Id, request.ShopId);
        var role = await roleRepository.GetAsync(roleByIdSpecification, cancellationToken) ?? throw new NotFoundException("The role with such id doesn't exists.");
        request.Model.Adapt(role);

        await roleRepository.UpdateAsync(role, cancellationToken);
    }
}