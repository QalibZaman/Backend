using System.ComponentModel.DataAnnotations;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Extensions;
using NettoRetail.Application.Models.Admin;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Permissions;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.IRoleService
{
    public class RoleService(
        IAccountRepository accountRepository,
        IPermissionRepository permissionRepository,
        IRoleRepository roleRepository) : IRoleService
    {
        public bool ArePermissionsEqual(ICollection<PermissionRoleEntity> existingPermissions, ICollection<PermissionRoleEntity> newPermissions)
        {
            if (existingPermissions is null || newPermissions is null)
                return false;

            var existingPermissionIds = existingPermissions
            .Select(pr => pr.PermissionId)
            .ToHashSet();

            return existingPermissionIds.SetEquals(newPermissions.Select(pr => pr.PermissionId));
        }

        public async Task<RoleEntity> CreatePlatformAdminRole(
            CreatePlatformAdminModel model,
            CancellationToken cancellationToken)
        {
            model.Normalize();

            var emailSpec = new GetAccountByEmailSpecification(model.Email);
            if (await accountRepository.Exists(emailSpec, cancellationToken))
                throw new ValidationException("Email is already in use");

            if (model.Permissions == null || !model.Permissions.Any())
                throw new BadRequestException("At least one permission must be selected.");

            var newRole = await GetOrCreatePlatformAdminRole(model.Permissions, cancellationToken);

            return newRole;
        }

        public async Task<RoleEntity> GetOrCreatePlatformAdminRole(
            ICollection<PlatformAdminPermission> adminPermissions,
            CancellationToken cancellationToken)
        {
            var permissionTitles = adminPermissions
                .Select(p => p.GetEnumMemberValue()?.Trim().ToLowerInvariant())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct()
                .ToList();

            var permissionIDs = await permissionRepository.GetIdsBySpecAsync(
                new GetPermissionsByTitlesSpecification(permissionTitles), cancellationToken);

            var spec = new GetRoleByExactPermissionsSpecification(permissionIDs);
            var existingRoles = await roleRepository.GetAllBySpecAsync(spec, cancellationToken);
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

        public bool IsLocked(RoleEntity role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            if (!role.LockedByUserId.HasValue || !role.LockedAtUtc.HasValue)
                return false;

            var lockActive = role.LockedAtUtc.Value >= DateTime.UtcNow - RoleLockConstants.Ttl;

            return lockActive;
        }
    }
}
