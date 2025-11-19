namespace NettoRetail.Application.Models.Response;

public class GetFavouriteResponse
{
    public IEnumerable<InventoryItemCardResponse> Items { get; set; } = new List<InventoryItemCardResponse>();
    public int AvailableItemCount { get; set; }
    public decimal TotalPriceAvailable { get; set; }
}
