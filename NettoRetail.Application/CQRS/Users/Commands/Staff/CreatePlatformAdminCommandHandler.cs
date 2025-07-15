using MediatR;
using Microsoft.AspNetCore.Identity;
using NettoRetail.Application.CQRS.Users.Commands.Password;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Permissions;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using System.ComponentModel.DataAnnotations;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record CreatePlatformAdminCommand(
    CreatePlatformAdminRequest Model,
    Guid SuperAdminId
) : IRequest<CreatePlatformAdminResponse>;

public class CreatePlatformAdminCommandHandler(
    IAccountRepository accountRepository,
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IMediator mediator
) : IRequestHandler<CreatePlatformAdminCommand, CreatePlatformAdminResponse>
{
    public async Task<CreatePlatformAdminResponse> Handle(
        CreatePlatformAdminCommand request,
        CancellationToken cancellationToken)
    {
        var model = request.Model;

        // 1) Email uniqueness → 409
        var existing = await accountRepository.GetAsync(
            new GetAccountByEmailSpecification(model.Email),
            cancellationToken);
        if (existing is not null)
            throw new ConflictException(
                "A platform admin with this email already exists.");

        // 2) Load & validate permissions → 400
        var perms = await permissionRepository.GetListAsync(
            new GetPermissionsByTitlesSpecification(model.Permissions),
            cancellationToken);
        if (perms.Count != model.Permissions.Count())
            throw new ValidationException(
                "One or more invalid permissions selected.");

        var role = new RoleEntity
        {
            Name = $"PlatformAdmin_{Guid.NewGuid():N}",
            Description = "Dynamic role for platform admin",
            ShopId = null
        };
        foreach (var p in perms)
            role.PermissionRoles.Add(new PermissionRoleEntity
            {
                PermissionId = p.Id,
                Role = role
            });

        var account = new AccountEntity
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = model.Email.Trim().ToLowerInvariant(),
            PhoneNumber = model.PhoneNumber?.Trim(),
            Status = StaffStatusEnum.CreatePassword,
            Role = role,
            PasswordHash = string.Empty
        };

        using var tx = unitOfWork.BeginTransaction();
        await roleRepository.AddAsync(role, cancellationToken);
        await accountRepository.AddAsync(account, cancellationToken);
        tx.Commit();

        return new CreatePlatformAdminResponse
        {
            AccountId = account.Id,
            Email = account.Email,
            Status = account.Status,
            AssignedPermissions = perms.Select(p => p.Title).ToList()
        };
    }
}
