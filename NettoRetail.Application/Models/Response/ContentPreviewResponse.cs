namespace NettoRetail.Application.Models.Response;

public class ContentPreviewResponse
{
    public required int ShopId { get; set; }
    public required string SectionType { get; set; }
    public required string Content { get; set; }
    public required bool IsVisible { get; set; }
}
