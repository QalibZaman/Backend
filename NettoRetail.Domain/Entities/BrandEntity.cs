namespace NettoRetail.Domain.Entities;

public class BrandEntity
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoImageUrl { get; set; }
    public ICollection<InventoryItemEntity>? InventoryItems { get; set; }
    public required int ShopId { get; set; }
    public ShopEntity? Shop { get; set; }
}
