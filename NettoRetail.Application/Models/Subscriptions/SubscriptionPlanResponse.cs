using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Subscriptions;
public class SubscriptionPlanResponse
{
    public required SubscriptionTypeEnum Name { get; set; }
    public required string Description { get; set; }
    public required string Motto { get; set; }
    public required int MonthlyPrice { get; set; }
}
