using NettoRetail.Domain.Entities;

public class PriceHistoryEntity
{
    public int Id { get; set; }
    public int SubscriptionPlanId { get; set; }
    public int Price { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }

    public SubscriptionPlanEntity SubscriptionPlan { get; set; } = default!;
}
