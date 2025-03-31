using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Role;

public class GetRolesByShopIdSpecification : Specification<RoleEntity>
{
    public GetRolesByShopIdSpecification(int shopId)
        : base(model => model.ShopId == shopId || model.ShopId == null) { }
}