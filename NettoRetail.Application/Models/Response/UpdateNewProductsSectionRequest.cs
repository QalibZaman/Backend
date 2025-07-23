using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response;

public class UpdateNewProductsSectionRequest
{
  public string Name { get; set; } = default!;
  public string? Preview { get; set; }
  public bool IsVisible { get; set; }
  public SelectionTypeEnum SelectionType { get; set; }
  public List<Guid> InventoryItemIds { get; set; } = new();
  public string UpdatedBy { get; set; } = default!;
}
