namespace NettoRetail.Application.Models.Response;

public class InventoryItemCardResponse
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public IEnumerable<string> MediaUrls { get; set; } = new List<string>();
    public string ThumbnailUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? PromotionPrice { get; set; }
    public int AvailableAmount { get; set; }
    public bool IsOnSale { get; set; }
    public bool IsNew { get; set; }
    public bool IsOutOfStock { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime LastUpdatedDate { get; set; }
}