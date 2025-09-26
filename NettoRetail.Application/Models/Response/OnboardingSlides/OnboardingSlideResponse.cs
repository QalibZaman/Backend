namespace NettoRetail.Application.Models.Response.OnboardingSlides;

public class OnboardingSlideResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public int Order { get; set; }
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
}