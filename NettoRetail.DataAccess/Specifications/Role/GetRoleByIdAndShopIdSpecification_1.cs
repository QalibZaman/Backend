using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Role;

public class GetRoleByIdAndShopIdSpecification : Specification<RoleEntity>
{
    public GetRoleByIdAndShopIdSpecification(int? id, int shopId)
        : base(role => role.Id == id && role.ShopId == shopId)
    {
        AddInclude(role => role.PermissionRoles);
    }
}
