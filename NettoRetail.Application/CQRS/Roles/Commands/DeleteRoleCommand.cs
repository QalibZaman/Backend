using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Roles.Commands
{
    public record DeleteRoleCommand(int id, int shopId) : IRequest;

    public class DeleteRoleCommandHandler(IRoleRepository roleRepository) : IRequestHandler<DeleteRoleCommand>
    {
        public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await roleRepository.GetAsync(new GetRoleByIdAndShopIdSpecification(request.shopId, request.id),
                           cancellationToken)
                       ?? throw new NotFoundException("Role with provided id and shopId not found");

            if (role.Name.Contains("Admin")) throw new ConflictException("The admin role cannot be deleted.");

            await roleRepository.DeleteAsync(role, cancellationToken);
        }
    }
}