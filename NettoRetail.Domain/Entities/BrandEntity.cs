namespace NettoRetail.Domain.Entities;

public class BrandEntity
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string WebsiteUrl { get; set; }
    public required string LogoImageUrl { get; set; }
    public ICollection<InventoryItemEntity>? InventoryItems { get; set; }
    public required int ShopId { get; set; }
    public ShopEntity? Shop { get; set; }
}
