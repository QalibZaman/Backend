using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response;

public class InventoryItemResponse
{
    public Guid Id { get; set; }
    public required string ProductName { get; set; }
    public required string ProductBrand { get; set; }
    public required string ProductCategory { get; set; }
    public string ProductUpc { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public required string Description { get; set; }
    public List<ProductDetailModel> ProductDetails { get; set; }
}
