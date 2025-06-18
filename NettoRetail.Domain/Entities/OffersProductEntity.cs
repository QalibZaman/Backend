namespace NettoRetail.Domain.Entities;

public class OffersProductEntity
{
    public int Id { get; set; }
    public required int OfferId { get; set; }
    public OfferEntity Offer { get; set; } = null!;
    public required Guid InventoryItemId { get; set; }
    public InventoryItemEntity InventoryItem { get; set; } = null!;

    public required int DisplayOrder { get; set; }
}
