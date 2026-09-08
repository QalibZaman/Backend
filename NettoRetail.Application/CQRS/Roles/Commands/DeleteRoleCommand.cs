using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Roles.Commands
{
    public record DeleteRoleCommand(int id) : IRequest;
    public class DeleteRoleCommandHandler(IRoleRepository roleRepository) : IRequestHandler<DeleteRoleCommand>
    {
        public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var specificationBuilder = new SpecificationBuilder<RoleEntity>();
            specificationBuilder.
                AddCriteria((role) => role.Id.Equals(request.id));

            var role = await roleRepository.GetAsync(specificationBuilder.Build(), cancellationToken)
                ?? throw new NotFoundException($"Role with id = {request.id} not found");

            await roleRepository.DeleteAsync(role, cancellationToken);
        }
    }
}
