namespace NettoRetail.Application.Models.InventoryItem;

public class GetFilteredInventoryItemsModel
{
    public string? ProductName { get; set; }
    public string? ProductLocation { get; set; }
    public int CategoryId { get; set; }
    public long AvailableAmount { get; set; }
    public long ReservedAmount { get; set; }
    public long OnHandAmount { get; set; }
    public decimal Price { get; set; }
    public string? status { get; set; } // TODO Wtf is status
}
