using System.ComponentModel.DataAnnotations;
using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Extensions;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Permissions;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications;

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
		var model = new CreatePlatformAdminModel(request.Model);
		model.Normalize();
		List<string> permissionTitles;
		var emailSpec = new GetAccountByEmailSpecification(model.Email);
		var existingEmail = await accountRepo.Exists(emailSpec, cancellationToken);

		if (existingEmail)
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
			Name = $"{CreatePlatformAdminConstants.PlatformAdminPrefix}_{Guid.NewGuid():N}",
			Description = CreatePlatformAdminConstants.PlatformAdminDescription,
			ShopId = null,
			PermissionRoles = matchingIds
				.Select(id => new PermissionRoleEntity { PermissionId = id })
				.ToList()
		};

		var account = model.Adapt<AccountEntity>();
		account.Role = role;

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
