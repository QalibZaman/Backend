using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Roles.Commands
{
    public record DeleteRoleCommand(int id, int shopId) : IRequest;
    public class DeleteRoleCommandHandler(IRoleRepository roleRepository) : IRequestHandler<DeleteRoleCommand>
    {
        public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var specificationBuilder = new SpecificationBuilder<RoleEntity>();
            specificationBuilder.
                AddCriteria((role) => role.Id.Equals(request.id)).
                AddCriteria((role) => role.ShopEntityId.Equals(request.shopId));

            var role = await roleRepository.GetAsync(specificationBuilder.Build(), cancellationToken)
                ?? throw new NotFoundException($"Role with id = {request.id} and shopId = {request.shopId} not found");

            if (role.Name.Contains("Admin")) throw new ConflictException("The admin role cannot be deleted.");

            await roleRepository.DeleteAsync(role, cancellationToken);
        }
    }
}
