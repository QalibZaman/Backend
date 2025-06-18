using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class OfferEntity
{
    public int Id { get; set; }
    public required int ShopId { get; set; }
    public required string Name { get; set; }
    public string? Preview { get; set; }
    public required bool IsVisible { get; set; }
    public SelectionTypeEnum SelectionType { get; set; } = SelectionTypeEnum.Static;

    public DateTime LastUpdated { get; set; }
    public string? UpdatedBy { get; set; }

    public ICollection<OffersProductEntity> Products { get; set; } = new List<OffersProductEntity>();
}
