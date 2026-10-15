using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Permissions
{
    public class GetValidPermissionsSpecification : Specification<PermissionEntity>
    {
        public GetValidPermissionsSpecification(int[] ids)
            : base(permission => ids.Contains(permission.Id))
        {
            AddSelectExpression(permission => permission.Id);
        }
    }
}
