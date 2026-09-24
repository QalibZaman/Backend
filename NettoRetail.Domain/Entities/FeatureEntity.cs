namespace NettoRetail.Domain.Entities
{
    public class FeatureEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public ICollection<SubscriptionPlanEntity>? SubscriptionPlans { get; set; }
    }
}