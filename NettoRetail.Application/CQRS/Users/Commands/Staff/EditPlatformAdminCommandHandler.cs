using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Services.IRoleService;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record EditPlatformAdminCommand(
    Guid PlatformAdminId,
    Guid EditorId,
    EditPlatformAdminRequest Model) : IRequest<Unit>;

public class EditPlatformAdminCommandHandler(
   IStaffRepository staffRepository,
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
            throw new InvalidOperationException(HandlerConstants.Admin.CannotEditOwnAccountMessage);

        var accountByIdSpecification = new GetStaffByIdSpecification(request.PlatformAdminId)
            .AddInclude(x => x.Role)
            .AddInclude(x => x.Role.PermissionRoles);

        var account = await staffRepository.GetAsync(accountByIdSpecification, cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Admin.PlatformAdminNotFoundMessage);

        var roleWithNewPermissions = await roleService.GetOrCreatePlatformAdminRole(request.Model.Permissions, cancellationToken);

        var arePermissionsEqual = roleService.ArePermissionsEqual(account.Role.PermissionRoles, roleWithNewPermissions.PermissionRoles);
        if (arePermissionsEqual) throw new BadRequestException(HandlerConstants.Admin.NoChangesToSaveMessage);

        using var transaction = unitOfWork.BeginTransaction();

        var role = await roleRepository.AttachOrAddAsync(roleWithNewPermissions, cancellationToken);
        account.Role = role;
        await staffRepository.UpdateAsync(account, cancellationToken);

        transaction.Commit();

        return Unit.Value;
    }
}
