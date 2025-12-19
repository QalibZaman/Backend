using System.Runtime.Intrinsics.X86;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrderByCustomerIdAndShopIdSpecification : Specification<OrderEntity>
{
    public GetOrderByCustomerIdAndShopIdSpecification(Guid customerId, int shopId)
        : base(x => x.CustomerId == customerId && x.ShopId == shopId) { }
}