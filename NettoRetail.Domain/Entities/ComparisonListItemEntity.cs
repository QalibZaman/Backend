namespace NettoRetail.Domain.Entities;

public class ComparisonListItemEntity
{
    public int Id { get; set; }

    public Guid InventoryItemId { get; set; }
    public required InventoryItemEntity InventoryItem { get; set; }

    public int ComparisonListId { get; set; }
    public required ComparisonListEntity ComparisonList { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}