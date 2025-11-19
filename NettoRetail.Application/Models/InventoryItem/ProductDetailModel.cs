using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.InventoryItem;
public class ProductDetailModel
{
    public required string Name { get; set; }
    public required ProductDetailTypeEnum Type { get; set; }
    public ProductDetailUnitEnum Unit { get; set; }
    public required string Value { get; set; }
    public int OrderIndex { get; set; }
}
