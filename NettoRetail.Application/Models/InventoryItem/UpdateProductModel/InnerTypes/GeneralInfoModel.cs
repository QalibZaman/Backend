namespace NettoRetail.Application.Models.InventoryItem;

public class GeneralInfoModel
{
    public bool? HasVariations { get; init; }
    public int? BrandId { get; init; }
    public string? ProductName { get; init; }
    public int? CategoryId { get; init; }
    public string? ProductUpc { get; init; }
    public string? Description { get; init; }
    public bool? IsActive { get; init; }
}