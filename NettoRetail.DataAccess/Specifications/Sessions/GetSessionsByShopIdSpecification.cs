using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Sessions;
public class GetSessionsByShopIdSpecification : Specification<SessionEntity>
{
    public GetSessionsByShopIdSpecification(int shopId) : base(b => b.Account.ShopId == shopId) { }
}
