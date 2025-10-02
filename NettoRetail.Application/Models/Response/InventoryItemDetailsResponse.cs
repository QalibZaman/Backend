namespace NettoRetail.Application.Models.Response;

public class InventoryItemDetailsResponse
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<string> Categories { get; set; } = [];
    public IEnumerable<string> MediaUrls { get; set; } = [];
    public decimal Price { get; set; }
    public decimal? PromotionPrice { get; set; }
    public int AvailableAmount { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public bool IsOnSale { get; set; }
    public bool IsOutOfStock { get; set; }
    public bool IsNew { get; set; }
}