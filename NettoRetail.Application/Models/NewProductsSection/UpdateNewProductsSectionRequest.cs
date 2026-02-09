using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.NewProductsSection;

public class UpdateNewProductsSectionRequest
{
    public required string Name { get; set; } = default!;
    public required bool IsVisible { get; set; }
    public NewProductsSelectionTypeEnum SelectionType { get; set; }
    public List<Guid> InventoryItemIds { get; set; } = new();
    public string UpdatedBy { get; set; } = default!;
}
