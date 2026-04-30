using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByDeliveryTypeEnumSpecification : Specification<OrderEntity>
{
    public GetOrdersByDeliveryTypeEnumSpecification(List<OrderDeliveryTypeEnum> types)
        : base(x => x.Delivery != null && x.Delivery.DeliveryType.HasValue && types.Contains(x.Delivery.DeliveryType.Value)) { }
}
