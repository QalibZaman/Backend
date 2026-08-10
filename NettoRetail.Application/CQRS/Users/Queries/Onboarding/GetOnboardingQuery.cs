using MediatR;

namespace NettoRetail.Application.CQRS.Users.Queries.Onboarding
{
    public record GetOnboardingQuery : IRequest<OnboardingResponse?>;
}
