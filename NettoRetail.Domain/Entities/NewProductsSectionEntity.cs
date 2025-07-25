using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class NewProductsSectionEntity
{
    public int Id { get; set; }
    public required int ShopId { get; set; }
    public required string Name { get; set; } = string.Empty;
    public string? Preview { get; set; }
    public required bool IsVisible { get; set; }
    public NewProductsSelectionTypeEnum SelectionType { get; set; } = NewProductsSelectionTypeEnum.Static;
    public DateTime LastUpdated { get; set; }
    public string? UpdatedBy { get; set; }
    public ICollection<NewProductsProductEntity> Products { get; set; } = new List<NewProductsProductEntity>();
}
