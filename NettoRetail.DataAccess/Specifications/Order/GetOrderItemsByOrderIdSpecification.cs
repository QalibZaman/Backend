using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.OrderItems;

public class GetOrderItemsByOrderIdSpecification : Specification<OrderItemEntity>
{
    public GetOrderItemsByOrderIdSpecification(int orderId)
        : base(x => x.OrderId == orderId) { }
}
