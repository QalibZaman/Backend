using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Permissions;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record EditPlatformAdminCommand(
    Guid PlatformAdminId,
    Guid EditorId,
    EditPlatformAdminRequest Model) : IRequest;

 public class EditPlatformAdminCommandHandler(
    IAccountRepository accountRepository,
    IPermissionRepository permissionRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<EditPlatformAdminCommand>
{
  public async Task Handle(
      EditPlatformAdminCommand request,
      CancellationToken cancellationToken)
  {
    var model = request.Model;

    if (request.EditorId == request.PlatformAdminId)
      throw new InvalidOperationException("You cannot edit your own account.");

    //if (model.AssignedPermissions == null || !model.AssignedPermissions.Any())
    //  throw new BadRequestException("At least one permission must be selected");

    var accountByIdSpecification = new GetAccountByIdSpecification(request.PlatformAdminId);
    accountByIdSpecification.AddInclude(x => x.Role.PermissionRoles);
    var account = await accountRepository.GetAsync(accountByIdSpecification, cancellationToken) ?? throw new NotFoundException("Account no found.");

    var newPermissionIds = (await permissionRepository
        .GetIdsBySpecAsync(
            new GetPermissionsByTitlesSpecification(model.AssignedPermissions),
            cancellationToken))
        .ToHashSet();

    var existingPermissionIds = account.Role.PermissionRoles
        .Select(pr => pr.PermissionId)
        .ToHashSet();

    if (newPermissionIds.SetEquals(existingPermissionIds))
      throw new BadRequestException("No changes to save");

    using var transaction = unitOfWork.BeginTransaction();

    account.Role.PermissionRoles.Clear();
    foreach (var id in newPermissionIds)
    {
      account.Role.PermissionRoles.Add(new PermissionRoleEntity
      {
        RoleId = account.Role.Id,
        PermissionId = id
      });
    }

    await accountRepository.UpdateAsync(account, cancellationToken);
    transaction.Commit();
  }
}
