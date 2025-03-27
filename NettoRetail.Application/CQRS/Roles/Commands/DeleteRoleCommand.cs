using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Roles.Commands;

public record DeleteRoleCommand(int Id) : IRequest;

public class DeleteRoleCommandHandler(IRoleRepository roleRepository, IAccountRepository accountRepository) : IRequestHandler<DeleteRoleCommand>
{
    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetRoleByIdSpecification(request.Id);

        var role = await roleRepository.GetAsync(specification, cancellationToken);

        if (role == null)
        {
            throw new NotFoundException($"There is no role with Id: {request.Id}");
        }

        var accountExistsSpecification = new GetAccountsByRoleIdSpecification(request.Id);
        var accounExists = await accountRepository.Exists(accountExistsSpecification, cancellationToken);
        if (accounExists)
        {
            throw new ConflictException("Cannot delete role. This role is currently assigned to active staff. Please reassign or remove staff before deletion.");
        }

        var deleteSpecification = new GetRoleByIdSpecification(role.Id);

        await roleRepository.DeleteAsync(deleteSpecification, cancellationToken);
    }
}