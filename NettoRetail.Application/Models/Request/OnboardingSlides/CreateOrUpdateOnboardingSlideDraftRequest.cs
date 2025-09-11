namespace NettoRetail.Application.Models.Request.OnboardingSlides;

public class CreateOrUpdateOnboardingSlideDraftRequest
{
    public int? SlideId { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
}
