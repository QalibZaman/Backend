using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class ContentEntity
{
    public int Id { get; set; }
    public int ShopId { get; set; }
    public string SectionType { get; set; }
    public string? ContentUrl { get; set; }
    public bool IsVisible { get; set; }
    public DateTime LastUpdated { get; set; }
    public string UpdatedBy { get; set; }
}
