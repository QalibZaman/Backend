using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class DeliveryEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public int? OrderId { get; set; }
    public Guid? AddressId { get; set; }
    public OrderDeliveryTypeEnum? DeliveryType { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal PaymentTotal { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public CustomerEntity Customer { get; set; } = null!;
    public AddressEntity? Address { get; set; }
    public OrderEntity? Order { get; set; }
}
