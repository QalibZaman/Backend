using MediatR;

namespace NettoRetail.Application.CQRS.Users.Commands.Onboarding
{
    public record CompleteOnboardingCommand : IRequest;
}
