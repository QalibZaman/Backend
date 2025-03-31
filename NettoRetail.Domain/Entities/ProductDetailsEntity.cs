using System.Text.Json.Serialization;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class ProductDetailsEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Name { get; set; }
    public ProductDetailTypeEnum TypeEnum { get; set; }
    public ProductDetailUnitEnum? Unit { get; set; }
    public required object Value { get; set; }
    public required int OrderIndex { get; set; }
    public required Guid InventoryItemId { get; set; }
}
