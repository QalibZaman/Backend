using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Delivery;

public class SetDeliveryMethodRequest
{
    public OrderDeliveryTypeEnum? DeliveryType { get; set; }
}
