using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Role
{
    public class GetRoleByExactPermissionsSpecification : Specification<RoleEntity>
    {
        public GetRoleByExactPermissionsSpecification(IEnumerable<int> permissionIds)
       : base(role =>
           role.PermissionRoles.Count == permissionIds.ToList().Count &&
                role.PermissionRoles.All(pr => permissionIds.Contains(pr.PermissionId)))
        { }
    }
}
