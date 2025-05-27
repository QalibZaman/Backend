using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Request.Subscription;
public class ChangeSubscriptionRequest
{
    public SubscriptionTypeEnum NewPlan { get; set; }
    public string? PaymentMethodId { get; set; }
}