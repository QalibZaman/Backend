using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Delivery;

public class DeliveryResponse
{
    public Guid CustomerId { get; set; }
    public int? OrderId { get; set; }
    public Guid? AddressId { get; set; }
    public OrderDeliveryTypeEnum? DeliveryType { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal PaymentTotal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
