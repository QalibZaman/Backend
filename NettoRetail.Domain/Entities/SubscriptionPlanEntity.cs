using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class SubscriptionPlanEntity
{
    public int Id { get; set; }
    public SubscriptionTypeEnum Type { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required int MonthlyPrice { get; set; }
    public required string Motto { get; set; }
    public required string SubscriptionId { get; set; }

    public ICollection<ShopEntity> Shops { get; set; } = [];
    public ICollection<FeatureEntity>? Features { get; set; }
}
