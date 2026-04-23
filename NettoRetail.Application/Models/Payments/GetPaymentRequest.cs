namespace NettoRetail.Application.Models.Payments;

public class GetPaymentRequest
{
    public required string StripePaymentMethodId { get; set; }

}