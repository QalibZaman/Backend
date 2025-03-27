using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class ProductDetailsEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public ProductDetailTypeEnum TypeEnum { get; set; }
    public ProductDetailUnitEnum Unit { get; set; }
    public required object Value { get; set; }
    public required int OrderIndex { get; set; }
    public required Guid InventoryItemId { get; set; }
}
