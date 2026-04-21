using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Role;

public class GetRolesByIdsAndShopIdSpecification : Specification<RoleEntity>
{
    public GetRolesByIdsAndShopIdSpecification(IEnumerable<int> roleIds, int shopId)
        : this(roleIds.ToArray(), shopId)
    {
    }

    private GetRolesByIdsAndShopIdSpecification(int[] roleIds, int shopId)
        : base(role => roleIds.Contains(role.Id) && (role.ShopId == shopId || role.Id == RoleConstants.Admin))
    {
    }
}
