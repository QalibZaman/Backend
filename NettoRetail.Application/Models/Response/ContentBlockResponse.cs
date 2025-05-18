namespace NettoRetail.Application.Models.Response;

public class ContentBlockResponse
{
    public int Id { get; set; }
    public string SectionType { get; set; }
    public string? ContentUrl { get; set; }
    public bool IsVisible { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? UpdatedBy { get; set; }
}