namespace NettoRetail.Application.Models.Response;

public class InventoryItemListResponse
{
    public Guid Id { get; set; }
    public required string ProductName { get; set; }
    public required string ProductCategory { get; set; }
    public required string ProductLocation { get; set; }
    public long ProductUpc { get; set; }
    public decimal Price { get; set; }
    public bool IsPromoted { get; set; }
    public decimal? PromotionPrice { get; set; }
    public int AvailableAmount { get; set; }
    public int ReservedAmount { get; set; }
    public int OnHandAmount { get; set; }
    public required DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
    public required string Status { get; set; }
    public required string BrandName { get; set; }
}
