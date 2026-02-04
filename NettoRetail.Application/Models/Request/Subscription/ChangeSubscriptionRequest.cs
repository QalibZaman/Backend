using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Request.Subscription;
public class ChangeSubscriptionRequest
{
    public SubscriptionTypeEnum NewPlan { get; set; }
    public required string PaymentMethodId { get; set; }
}