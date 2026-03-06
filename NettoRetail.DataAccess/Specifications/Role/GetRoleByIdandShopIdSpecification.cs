using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Role;

public class GetRoleByIdandShopIdSpecification : Specification<RoleEntity>
{
    public GetRoleByIdandShopIdSpecification(int roleId, int shopId)
        : base(role => role.Id == roleId && role.ShopId == shopId) { }
}