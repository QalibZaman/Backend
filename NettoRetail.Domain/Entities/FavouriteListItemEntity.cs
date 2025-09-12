namespace NettoRetail.Domain.Entities;

public class FavouriteListItemEntity
{
    public Guid AccountId { get; set; }
    public AccountEntity? Account { get; set; }
    public Guid InventoryItemId { get; set; }
    public InventoryItemEntity? InventoryItem { get; set; }
}
