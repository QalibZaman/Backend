using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.PermissionRole;

public class GetPermissionsByRoleIdSpecification : Specification<PermissionRoleEntity>
{
    public GetPermissionsByRoleIdSpecification(int roleId)
        : base(model => model.RoleId == roleId)
    {
            AddInclude(model => model.Permission);
    }
}
