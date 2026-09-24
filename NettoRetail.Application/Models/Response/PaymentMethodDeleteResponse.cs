namespace NettoRetail.Application.Models.Response;

public class PaymentMethodDeleteResponse
{
    public required bool IsAutoRenewalDisabled { get; set; }
    public DateTime? SubscriptionExpiresAt { get; set; }
}