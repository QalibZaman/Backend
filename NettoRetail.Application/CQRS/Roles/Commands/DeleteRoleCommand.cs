using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services.IRoleService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Roles.Commands;

public record DeleteRoleCommand(int Id, int ShopId) : IRequest;

public class DeleteRoleCommandHandler(IRoleService roleService, IRoleRepository roleRepository,
    IAccountRepository accountRepository, IShopRepository shopRepository) : IRequestHandler<DeleteRoleCommand>
{
    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var shopExists = await shopRepository.Exists(new GetShopByIdSpecification(request.ShopId), cancellationToken);
        if (!shopExists)
        {
            throw new NotFoundException($"No shop with Id: {request.ShopId}");
        }

        if (request.Id == RoleConstants.Admin) throw new ConflictException("The admin role cannot be deleted.");

        var specification = new GetRoleByShopIdSpecification(request.Id, request.ShopId);

        var role = await roleRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException("The role with such id doesn't exist.");

        if (roleService.IsLocked(role))
        {
            throw new ConflictException(
                "Cannot delete role. This role is currently being modified by another user.");
        }

        var accountExistsSpecification = new GetAccountsByRoleIdSpecification(request.Id);
        var accounExists = await accountRepository.Exists(accountExistsSpecification, cancellationToken);
        if (accounExists)
            throw new ConflictException("Cannot delete role. This role is currently assigned to active staff. Please reassign or remove staff before deletion.");

        await roleRepository.DeleteAsync(role, cancellationToken);
    }
}