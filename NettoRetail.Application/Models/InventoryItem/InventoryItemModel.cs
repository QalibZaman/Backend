namespace NettoRetail.Application.Models.InventoryItem;

public class InventoryItemModel
{
    public required string ProductName { get; set; }
    public required string ProductCategory { get; set; }
    public long ProductUpc { get; set; }
    public decimal Price { get; set; }
    public required string Description { get; set; }
}