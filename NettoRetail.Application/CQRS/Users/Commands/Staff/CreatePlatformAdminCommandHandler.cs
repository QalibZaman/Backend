using System.ComponentModel.DataAnnotations;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Extensions;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Permissions;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record CreatePlatformAdminCommand(
    CreatePlatformAdminRequest Model,
    Guid SuperAdminId
) : IRequest<CreatePlatformAdminResponse>;

public class CreatePlatformAdminCommandHandler(IAccountRepository accountRepo,
    IRoleRepository roleRepo,
    IPermissionRepository permissionRepo,
    IUnitOfWork unitOfWork)
        : IRequestHandler<CreatePlatformAdminCommand, CreatePlatformAdminResponse>
{
    public async Task<CreatePlatformAdminResponse> Handle(
        CreatePlatformAdminCommand request,
        CancellationToken cancellationToken)
    {
        var model = request.Model;
        var email = model.Email.Trim().ToLowerInvariant();
        var firstName = model.FirstName.Trim();
        var lastName = model.LastName.Trim();
        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        List<string> permissionTitles;

        var existingEmails = await accountRepo.GetAccountEmailsAsync(
            new Specification<AccountEntity>(a => a.Email == normalizedEmail),
            cancellationToken);

        if (existingEmails.Any())
            throw new ValidationException($"Email is already in use.");

        if (model.Permissions == null || !model.Permissions.Any())
            {
              throw new BadRequestException("At least one permission must be selected.");
            }
            else
            {
              permissionTitles = [.. model.Permissions.Select(p => p.GetEnumMemberValue()?.Trim().ToLowerInvariant()).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct()];
            }

        var matchingIds = (await permissionRepo
            .GetIdsBySpecAsync(
                new GetPermissionsByTitlesSpecification(permissionTitles),
                cancellationToken))
            .ToList();

        var role = new RoleEntity
        {
            Name = $"PlatformAdmin_{Guid.NewGuid():N}",
            Description = "Dynamic role for platform admin",
            ShopId = null,
            PermissionRoles = matchingIds
                .Select(id => new PermissionRoleEntity { PermissionId = id })
                .ToList()
        };

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

        return new CreatePlatformAdminResponse
        {
            AccountId = account.Id,
            Email = account.Email,
            Status = account.Status,
            Permissions = model.Permissions
        };
    }
}
