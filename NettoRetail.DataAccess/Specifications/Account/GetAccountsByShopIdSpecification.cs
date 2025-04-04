using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Account;
public class GetAccountsByShopIdSpecification : Specification<AccountEntity>
{
    public GetAccountsByShopIdSpecification(int shopId)
        : base(model => model.ShopId == shopId) { }
}
