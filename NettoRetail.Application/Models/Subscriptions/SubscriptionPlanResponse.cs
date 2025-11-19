namespace NettoRetail.Application.Models.Subscriptions;
public class SubscriptionPlanResponse
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Motto { get; set; }
    public required int MonthlyPrice { get; set; }
    public bool IsActive { get; set; } = false;
}
