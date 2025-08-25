using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Role;

public class GetRoleByShopIdOrAdminIdSpecification : Specification<RoleEntity>
{
    public GetRoleByShopIdOrAdminIdSpecification(int? id, int shopId)
        : base(role => role.Id == id && (role.ShopId == shopId || id == RoleConstants.Admin))
    {
        AddInclude(role => role.PermissionRoles);
    }
}
