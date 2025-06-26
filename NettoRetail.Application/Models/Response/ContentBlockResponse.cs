namespace NettoRetail.Application.Models.Response;

public class ContentBlockResponse
{
    public required int Id { get; set; }
    public required string SectionType { get; set; }
    public string? Content { get; set; }
    public required bool IsVisible { get; set; }
    public required DateTime LastUpdated { get; set; }
    public required string UpdatedBy { get; set; }
}