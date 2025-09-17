namespace NettoRetail.Application.Models.Response.OnboardingSlides;

public class OnboardingSlideDraftResponse
{
    public int Id { get; set; }
    public int? SlideId { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime DateSaved { get; set; }
}