using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Request.Subscription;
public class ChangeSubscriptionRequest
{
    public required SubscriptionTypeEnum NewPlan { get; set; }
    public required string StripePaymentMethodId { get; set; }
}