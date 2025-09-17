namespace NettoRetail.Application.Models.Request.OnboardingSlides;

public class CreateOnboardingSlideRequest
{
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public bool IsVisible { get; set; }
}