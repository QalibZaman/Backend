namespace NettoRetail.Domain.Entities.JunctionEntities;

public class ItemComparisonItemEntity
{
    public required Guid InventoryItemId { get; set; }
    public InventoryItemEntity InventoryItem { get; set; } = null!;
    public required Guid ComparisonId { get; set; }
    public ItemComparisonEntity Comparison { get; set; } = null!;
}
