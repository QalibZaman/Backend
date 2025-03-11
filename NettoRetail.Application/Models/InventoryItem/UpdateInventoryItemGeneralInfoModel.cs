namespace NettoRetail.Application.Models.InventoryItem;

public class UpdateInventoryItemGeneralInfoModel
{
    public required string ProductName { get; set; }
    public required string ProductCategory { get; set; }
    public long ProductUpc { get; set; }
    public required string Description { get; set; }
    public bool IsActive { get; set; }
}