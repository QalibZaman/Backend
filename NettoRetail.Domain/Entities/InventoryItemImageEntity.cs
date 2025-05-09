namespace NettoRetail.Domain.Entities;

public class InventoryItemImageEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string BlobUrl { get; set; }

    public int Order { get; set; }

    public Guid InventoryItemId { get; set; }

    public InventoryItemEntity InventoryItem { get; set; } = null!;
}
