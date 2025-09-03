using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Role;

public class GetRoleByShopIdSpecification : Specification<RoleEntity>
{
    public GetRoleByShopIdSpecification(int? id, int shopId)
        : base(role => role.Id == id && role.ShopId == shopId)
    {
        AddInclude(role => role.PermissionRoles);
    }
}
