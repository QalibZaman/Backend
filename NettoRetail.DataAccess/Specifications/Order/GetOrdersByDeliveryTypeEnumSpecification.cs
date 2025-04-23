using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByDeliveryTypeEnumSpecification : Specification<OrderEntity>
{
    public GetOrdersByDeliveryTypeEnumSpecification(List<OrderDeliveryTypeEnum> types)
        : base(x => types.Contains(x.DeliveryType)) { }
}
