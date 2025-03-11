namespace NettoRetail.Application.Models.InventoryItem;

public class UpdateInventoryItemGeneralInfoModel
{
    public required string ProductName { get; set; }
    public required int CategoryId { get; set; }
    public required long ProductUpc { get; set; }
    public required string Description { get; set; }
    public required bool IsActive { get; set; }
}