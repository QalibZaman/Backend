namespace NettoRetail.Domain.Entities;

public class SubscriptionPlanEntity
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public required string StripePriceId { get; set; }

    public ICollection<SubscriptionEntity> Subscriptions { get; set; } = [];
}