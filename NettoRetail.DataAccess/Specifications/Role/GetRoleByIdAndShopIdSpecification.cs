using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Role;

public class GetRoleByIdAndShopIdSpecification : Specification<RoleEntity>
{
    public GetRoleByIdAndShopIdSpecification(int shopId, int id)
        : base(model => model.ShopId == shopId && model.Id == id)
    {
    }
}