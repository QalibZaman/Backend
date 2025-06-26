namespace NettoRetail.Application.Models.Request.Content;

public class UpdateContentRequest
{
    public required bool IsVisible { get; set; }
    public required string Content { get; set; }
}
