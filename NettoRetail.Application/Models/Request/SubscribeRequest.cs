namespace NettoRetail.Application.Models.Request;

public class SubscribeRequest
{
    public required int SubscriptionPlanId { get; set; }
    public required string PaymentMethodId { get; set; }
}