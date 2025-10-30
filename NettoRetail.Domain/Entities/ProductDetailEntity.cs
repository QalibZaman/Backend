using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class ProductDetailEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; } = string.Empty;
    public ProductDetailTypeEnum TypeEnum { get; set; }
    public ProductDetailUnitEnum? Unit { get; set; }
    public required string Value { get; set; } = string.Empty;
    public required Guid InventoryItemId { get; set; }

    public InventoryItemEntity? InventoryItem { get; set; }
}
