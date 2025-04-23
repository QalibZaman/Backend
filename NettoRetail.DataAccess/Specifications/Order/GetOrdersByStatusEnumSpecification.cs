using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByStatusEnumSpecification : Specification<OrderEntity>
{
    public GetOrdersByStatusEnumSpecification(List<OrderStatusEnum> statuses)
        : base(x => statuses.Contains(x.OrderStatus)) { }
}
