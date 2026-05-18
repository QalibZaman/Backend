namespace NettoRetail.Application.Models.Delivery;

public class SetDeliveryAddressMappingModel
{
    public Guid AddressId { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal PaymentTotal { get; set; }
}
