using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Delivery;

public class SetDeliveryMethodMappingModel
{
    public OrderDeliveryTypeEnum DeliveryType { get; set; }
    public Guid? AddressId { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal PaymentTotal { get; set; }
}
