namespace NettoRetail.Application.Models.Response;

public class ContentBlockResponse
{
    public required int Id { get; set; }
    public required string SectionType { get; set; }
    public string? Content { get; set; }
    public required bool IsVisible { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string? UpdatedBy { get; set; }
}
