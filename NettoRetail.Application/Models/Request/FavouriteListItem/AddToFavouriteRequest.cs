namespace NettoRetail.Application.Models.Request.FavouriteListItem;

public class AddToFavouriteRequest
{
    public required Guid InventoryItemId { get; set; }
    public bool IsAll { get; init; } = false;
}
