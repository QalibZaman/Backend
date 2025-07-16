using MediatR;
using NettoRetail.Application.CQRS.Users.Commands.Password;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Permissions;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record CreatePlatformAdminCommand(
    CreatePlatformAdminRequest Model,
    Guid SuperAdminId
) : IRequest<CreatePlatformAdminResponse>;

public class CreatePlatformAdminCommandHandler(
        IAccountRepository accountRepo,
        IRoleRepository roleRepo,
        IPermissionRepository permissionRepo,
        IMediator mediator,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<CreatePlatformAdminCommand, CreatePlatformAdminResponse>
{
    public async Task<CreatePlatformAdminResponse> Handle(
        CreatePlatformAdminCommand request,
        CancellationToken cancellationToken)
    {
        var model = request.Model;

        var email = model.Email.Trim().ToLowerInvariant();
        var firstName = model.FirstName.Trim();
        var lastName = model.LastName.Trim();

        var matchingIds = (await permissionRepo
                .GetIdsBySpecAsync(
                    new GetPermissionsByTitlesSpecification(model.Permissions),
                    cancellationToken))
            .ToList();

        var role = new RoleEntity
        {
            Name = $"PlatformAdmin_{Guid.NewGuid():N}",
            Description = "Dynamic role for platform admin",
            ShopId = null
        };
        matchingIds.ForEach(id =>
            role.PermissionRoles.Add(new PermissionRoleEntity
            {
                PermissionId = id,
                Role = role
            }));

        var account = new AccountEntity
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = model.PhoneNumber?.Trim(),
            Status = StaffStatusEnum.CreatePassword,
            Role = role,
            PasswordHash = string.Empty
        };

        using var tx = unitOfWork.BeginTransaction();
        await roleRepo.AddAsync(role, cancellationToken);
        await accountRepo.AddAsync(account, cancellationToken);
        tx.Commit();

        // activation logic via SendPasswordResetLinkCommand
        //    await mediator.Send(
        //        new SendPasswordResetLinkCommand(account.Email),
        //        cancellationToken);

        return new CreatePlatformAdminResponse
        {
            AccountId = account.Id,
            Email = account.Email,
            Status = account.Status,
            AssignedPermissions = model.Permissions.ToList()
        };
    }
}
