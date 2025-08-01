using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.NewProductsSection;

 public class GetNewProductsSectionResponse
{
  public int Id { get; set; }
  public int ShopId { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Preview { get; set; }
  public bool IsVisible { get; set; }
  public string? SelectionType { get; set; }
  public DateTime LastUpdated { get; set; }
  public string? UpdatedBy { get; set; }

  public List<Guid> Products { get; set; } = [];
}
