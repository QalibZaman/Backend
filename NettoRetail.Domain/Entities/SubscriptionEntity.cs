using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class SubscriptionEntity
{
    public int Id { get; set; }

    public required Guid AccountId { get; set; }
    public StaffEntity? Account { get; set; }

    public required int ShopId { get; set; }
    public ShopEntity? Shop { get; set; }

    public required int SubscriptionPlanId { get; set; }
    public SubscriptionPlanEntity? SubscriptionPlan { get; set; }

    public required int PaymentMethodId { get; set; }
    public PaymentMethodEntity? PaymentMethod { get; set; }

    public required string StripeSubscriptionId { get; set; }

    public SubscriptionStatusEnum Status { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime NextRenewalAt { get; set; }

    public DateTime? CancelledAt { get; set; }
    public bool AutoRenewalEnabled { get; set; }
}