namespace NettoRetail.Application.Models.Payments;

public class PaymentMethodsResponse
{
    public int Id { get; set; }
    public required string StripePaymentMethodId { get; set; }
    public string FourLastCardNumbers { get; set; }
    public string CardBrand { get; set; }
    public long CardExpMonth { get; set; }
    public long CardExpYear { get; set; }
    public bool IsDefault { get; set; }
}