using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Roles.Commands;

public record DeleteRoleCommand(int Id, int ShopId) : IRequest;

public class DeleteRoleCommandHandler(IRoleRepository roleRepository) : IRequestHandler<DeleteRoleCommand>
{
    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetRoleByIdAndShopIdSpecification(request.Id, request.ShopId);

        var role = await roleRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Role.RoleNotFoundMessage);

        await roleRepository.DeleteAsync(role, cancellationToken);
    }
}