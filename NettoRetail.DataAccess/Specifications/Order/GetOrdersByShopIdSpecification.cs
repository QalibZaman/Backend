using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByShopIdSpecification : Specification<OrderEntity>
{
    public GetOrdersByShopIdSpecification(int shopId)
        : base(x => x.ShopId == shopId) { }
}
