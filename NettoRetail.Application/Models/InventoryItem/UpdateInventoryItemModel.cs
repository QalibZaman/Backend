namespace NettoRetail.Application.Models.InventoryItem;

public class UpdateInventoryItemModel
{
    public string? ProductName { get; set; }
    public int? CategoryId { get; set; }
    public long? ProductUpc { get; set; }

    public string? Description { get; set; }
    public bool? IsActive { get; set; }

    public int? AvailableAmount { get; set; }
    public int? ReservedAmount { get; set; }
    public int? OnHandAmount { get; set; }
    public decimal? Price { get; set; }
    public decimal? PromotionPrice { get; set; }
    public bool? IsPromoted { get; set; }
}
