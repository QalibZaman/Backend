using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response;

public class InventoryItemInfoResponse
{
    public required string ProductName { get; set; }
    public required long ProductUpc { get; set; }
    public required IEnumerable<string> ProductCategories { get; set; }
    public required decimal Price { get; set; }
    public decimal? PromotionPrice { get; set; }
    public required string Description { get; set; }
    public required DateTime LastUpdatedDate { get; set; }
    public required string ProductLocation { get; set; }
    public required string Status { get; set; }
    public required int AvailableAmount { get; set; }
    public required int ReservedAmount { get; set; }
    public required int OnHandAmount { get; set; }
}
