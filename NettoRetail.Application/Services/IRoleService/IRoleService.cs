using NettoRetail.Application.Models.Admin;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
namespace NettoRetail.Application.Services.IRoleService
{
    public interface IRoleService
    {
        Task<RoleEntity> CreatePlatformAdminRole(CreatePlatformAdminModel model, CancellationToken cancellationToken);
        Task<RoleEntity> GetOrCreatePlatformAdminRole(ICollection<PlatformAdminPermission> adminPermissions, CancellationToken cancellationToken);
        bool ArePermissionsEqual(ICollection<PermissionRoleEntity> permissions, ICollection<PermissionRoleEntity> permissions1);
    }
}
