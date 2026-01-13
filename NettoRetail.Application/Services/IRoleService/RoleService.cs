using System.ComponentModel.DataAnnotations;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Extensions;
using NettoRetail.Application.Models.Admin;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Permissions;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.Services.IRoleService
{
    public class RoleService : IRoleService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(
            IAccountRepository accountRepository,
            IPermissionRepository permissionRepository,
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork)
        {
            _accountRepository = accountRepository;
            _permissionRepository = permissionRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<RoleEntity> CreatePlatformAdminRole(
            CreatePlatformAdminModel model,
            CancellationToken cancellationToken)
        {
            model.Normalize();

            var emailSpec = new GetAccountByEmailSpecification(model.Email);
            if (await _accountRepository.Exists(emailSpec, cancellationToken))
                throw new ValidationException("Email is already in use");

            if (model.Permissions == null || !model.Permissions.Any())
                throw new BadRequestException("At least one permission must be selected.");

            var permissionTitles = model.Permissions
                .Select(p => p.GetEnumMemberValue()?.Trim().ToLowerInvariant())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct()
                .ToList();

            var permissionIDs = await _permissionRepository.GetIdsBySpecAsync(
                new GetPermissionsByTitlesSpecification(permissionTitles), cancellationToken);

            var spec = new GetRoleByExactPermissionsSpecification(permissionIDs);
            var existingRoles = await _roleRepository.GetAllBySpecAsync(spec, cancellationToken);
            var existingRole = existingRoles?.FirstOrDefault();

            if (existingRole != null)
                return existingRole;

            var newRole = new RoleEntity
            {
                Name = $"{CreatePlatformAdminConstants.PlatformAdminPrefix}_{Guid.NewGuid():N}",
                Description = CreatePlatformAdminConstants.PlatformAdminDescription,
                ShopId = null,
                PermissionRoles = permissionIDs
                    .Select(id => new PermissionRoleEntity { PermissionId = id })
                    .ToList()
            };

            return newRole;
        }

    }
}
