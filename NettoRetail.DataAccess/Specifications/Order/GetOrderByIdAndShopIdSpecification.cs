using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrderByIdAndShopIdSpecification : Specification<OrderEntity>
{
    public GetOrderByIdAndShopIdSpecification(int orderId, int shopId)
        : base(o => o.Id == orderId && o.ShopId == shopId)
    {
    }
}
