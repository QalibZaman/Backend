namespace NettoRetail.Application.Models.Request.OnboardingSlides;

public class UpdateOnboardingSlideRequest
{
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public bool IsVisible { get; set; }
}