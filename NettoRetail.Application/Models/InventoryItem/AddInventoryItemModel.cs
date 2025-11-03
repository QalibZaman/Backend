using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.InventoryItem;

public class AddInventoryItemModel
{
    public required string ProductName { get; set; }
    public required int BrandId { get; set; }
    public required int CategoryId { get; set; }
    public long ProductUpc { get; set; }
    public decimal Price { get; set; }
    public required string Description { get; set; }
    public List<ProductDetailModel> ProductDetails { get; set; }
}

public class ProductDetailModel
{
    public required string Name { get; set; }
    public required ProductDetailTypeEnum Type { get; set; }
    public ProductDetailUnitEnum Unit { get; set; }
    public required string Value { get; set; }
    public int OrderIndex { get; set; }
}