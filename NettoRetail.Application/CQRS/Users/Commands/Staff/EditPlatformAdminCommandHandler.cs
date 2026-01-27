using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Services.IRoleService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record EditPlatformAdminCommand(
    Guid PlatformAdminId,
    Guid EditorId,
    EditPlatformAdminRequest Model) : IRequest<Unit>;

public class EditPlatformAdminCommandHandler(
   IAccountRepository accountRepository,
   IRoleRepository roleRepository,
   IRoleService roleService,
   IUnitOfWork unitOfWork
) : IRequestHandler<EditPlatformAdminCommand, Unit>
{
    public async Task<Unit> Handle(
        EditPlatformAdminCommand request,
        CancellationToken cancellationToken)
    {
        if (request.EditorId == request.PlatformAdminId)
            throw new InvalidOperationException("You cannot edit your own account.");

        var accountByIdSpecification = new GetAccountByIdSpecification(request.PlatformAdminId)
            .AddInclude(x => x.Role)
            .AddInclude(x => x.Role.PermissionRoles);

        var account = await accountRepository.GetAsync(accountByIdSpecification, cancellationToken)
            ?? throw new NotFoundException("Account not found.");

        var roleWithNewPermissions = await roleService.GetOrCreatePlatformAdminRole(request.Model.Permissions, cancellationToken);

        var arePermissionsEqual = roleService.ArePermissionsEqual(account.Role.PermissionRoles, roleWithNewPermissions.PermissionRoles);
        if (arePermissionsEqual) throw new BadRequestException("No changes to save");

        using var transaction = unitOfWork.BeginTransaction();

        var role = await roleRepository.AttachOrAddAsync(roleWithNewPermissions, cancellationToken);
        account.Role = role;
        await accountRepository.UpdateAsync(account, cancellationToken);

        transaction.Commit();

        return Unit.Value;
    }
}
