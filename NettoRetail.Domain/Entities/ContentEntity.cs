using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class ContentEntity
{
    public required int Id { get; set; }
    public required int ShopId { get; set; }
    public required string SectionType { get; set; }
    public string? ContentUrl { get; set; }
    public required bool IsVisible { get; set; }
    public required DateTime LastUpdated { get; set; }
    public required string UpdatedBy { get; set; }
}
