namespace NettoRetail.Application.Models.Request.OnboardingSlides;

public class ReorderOnboardingSlidesRequest
{
    public List<SlideOrderItem> Slides { get; set; } = new();

    public class SlideOrderItem
    {
        public int Id { get; set; }
        public int Order { get; set; }
    }
}