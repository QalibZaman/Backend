using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Response.Brand;
using NettoRetail.Application.Models.Response.Category;

namespace NettoRetail.Application.Models.Response;

public class InventoryItemInfoFullResponse
{
    public required Guid Id { get; set; }
    public required string ProductName { get; set; }
    public required string ProductUpc { get; set; }
    public required decimal Price { get; set; }
    public required bool IsPromoted { get; set; }
    public decimal? PromotionPrice { get; set; }
    public required string Description { get; set; }
    public required DateTime LastUpdatedDate { get; set; }
    public required string ProductLocation { get; set; }
    public required string Status { get; set; }
    public required int AvailableAmount { get; set; }
    public required int ReservedAmount { get; set; }
    public required int OnHandAmount { get; set; }
    public required bool HasVariations { get; set; }
    public required BrandSummaryResponse Brand { get; set; }
    public required IEnumerable<InventoryItemImageDto> Images { get; set; } = [];
    public required IEnumerable<CategorySummaryResponse> ProductCategories { get; set; } = [];
    public required IEnumerable<ProductDetailsResponse> ProductDetails { get; set; } = [];
    public CategorySummaryResponse? MainCategory => ProductCategories.LastOrDefault();
}
