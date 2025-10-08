namespace NettoRetail.Domain.Entities;

public class RecentlyViewedProductEntity
{
    public Guid AccountId { get; set; }
    public AccountEntity? Account { get; set; }
    public Guid InventoryItemId { get; set; }
    public InventoryItemEntity? InventoryItem { get; set; }
    public DateTime ViewedAtUtc { get; set; }
}
