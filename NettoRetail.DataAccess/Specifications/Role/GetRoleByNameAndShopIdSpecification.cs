using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Role;

public class GetRoleByNameAndShopIdSpecification : Specification<RoleEntity>
{
    public GetRoleByNameAndShopIdSpecification(string name, int shopId)
        : base(r => r.Name.ToLower() == name.ToLower() && r.ShopId == shopId) { }
}
