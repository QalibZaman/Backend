namespace NettoRetail.Application.CQRS.Users.Queries.Onboarding
{
    public class OnboardingSlideResponse
    {
        public int SlideId { get; set; }

        public string? Title { get; set; }

        public string? ImageUrl { get; set; }

        public string? Headline { get; set; }

        public string? DetailedText { get; set; }
    }
}
