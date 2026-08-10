namespace NettoRetail.Application.CQRS.Users.Queries.Onboarding
{
    public class OnboardingResponse
    {
        public bool OnboardingSeen { get; set; }

        public List<OnboardingSlideResponse> Slides { get; set; } = [];
    }
}
