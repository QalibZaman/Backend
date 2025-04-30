using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;

public class GetAccountByShopIdAndRoleSpecification : Specification<AccountEntity>
{
    public GetAccountByShopIdAndRoleSpecification(int shopId, int roleId)
        : base(x => x.ShopId == shopId && x.RoleId == roleId) { }
}