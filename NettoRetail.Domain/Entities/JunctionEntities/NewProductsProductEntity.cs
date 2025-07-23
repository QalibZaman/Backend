namespace NettoRetail.Domain.Entities.JunctionEntities;

 public class NewProductsProductEntity
{
  public int Id { get; set; }
  public required int SectionId { get; set; }
  public NewProductsSectionEntity Section { get; set; } = null!;
  public required Guid InventoryItemId { get; set; }
  public InventoryItemEntity InventoryItem { get; set; } = null!;
  public required int DisplayOrder { get; set; }
}
