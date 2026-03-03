using NettoRetail.Domain.Entities;

public class InventoryItemImageEntity
{
    public Guid Id { get; set; }

    public required string BlobUrl { get; set; }

    public int Order { get; set; }

    public Guid InventoryItemId { get; set; }

    public InventoryItemEntity InventoryItem { get; set; } = null!;
}
