using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Permissions;

public class GetPermissionsWIthIdsSpecification : Specification<PermissionEntity>
{
    public GetPermissionsWIthIdsSpecification(int[] ids)
        : base(permission => ids.Contains(permission.Id)) { }
}
