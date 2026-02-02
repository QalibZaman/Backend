
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Offer;

public class UpdateOfferSectionRequest
{
    public required string Name { get; set; }
    public string? Preview { get; set; }
    public required bool IsVisible { get; set; }
    public required SelectionTypeEnum SelectionType { get; set; }
    public required List<Guid> InventoryItemIds { get; set; } = new List<Guid>();
}
