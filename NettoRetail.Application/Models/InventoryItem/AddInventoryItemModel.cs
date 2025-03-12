namespace NettoRetail.Application.Models.InventoryItem;

public class AddInventoryItemModel
{
    public required string ProductName { get; set; }
    public required int CategoryId { get; set; }
    public long ProductUpc { get; set; }
    public decimal Price { get; set; }
    public required string Description { get; set; }
}