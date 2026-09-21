namespace NettoRetail.Domain.Entities;

public class PaymentMethodEntity
{
    public int Id { get; set; }
    public required Guid AccountId { get; set; }
    public StaffEntity? Account { get; set; }
    public required string StripePaymentMethodId { get; set; }
    public required string CardBrand { get; set; }
    public required string FourLastCardNumbers { get; set; }
    public long CardExpMonth { get; set; }
    public long CardExpYear { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}