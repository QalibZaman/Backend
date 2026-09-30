using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Role;

public class GetRoleByIdAndShopId : Specification<RoleEntity>
{
    public GetRoleByIdAndShopId(int shopId, int id)
        : base(model => model.ShopEntityId == shopId && model.Id == id)
    {
    }
}