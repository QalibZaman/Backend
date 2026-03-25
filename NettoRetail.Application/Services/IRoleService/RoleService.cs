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
        IStaffRepository accountRepository,
        IPermissionRepository permissionRepository,
        IRoleRepository roleRepository) : IRoleService
    {
        public bool ArePermissionsEqual(ICollection<PermissionRoleEntity> existingPermissions,
            ICollection<PermissionRoleEntity> newPermissions)
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

            var emailSpec = new GetStaffByEmailSpecification(model.Email);
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

            if (permissionTitles.Count == 0)
                throw new BadRequestException("Selected permissions are invalid.");

            var permissionIDs = await permissionRepository.GetIdsBySpecAsync(
                new GetPermissionsByTitlesSpecification(permissionTitles), cancellationToken);

            if (permissionIDs.Count() == 0)
                throw new BadRequestException("Selected permissions are invalid.");

            var spec = new GetRoleByExactPermissionsSpecification(permissionIDs);
            var existingRoles = await roleRepository.GetAllBySpecAsync(spec, cancellationToken);
            var existingRole = existingRoles?
                .FirstOrDefault(role =>
                    role.Name.StartsWith(PlatformAdminConstants.PlatformAdminPrefix,
                        StringComparison.OrdinalIgnoreCase));

            if (existingRole != null)
                return existingRole;

            var newRole = new RoleEntity
            {
                Name = $"{PlatformAdminConstants.PlatformAdminPrefix}_{Guid.NewGuid():N}",
                Description = PlatformAdminConstants.PlatformAdminDescription,
                ShopId = null,
                PermissionRoles = permissionIDs
                    .Select(id => new PermissionRoleEntity { PermissionId = id })
                    .ToList()
            };

            return newRole;
        }
    }
}

