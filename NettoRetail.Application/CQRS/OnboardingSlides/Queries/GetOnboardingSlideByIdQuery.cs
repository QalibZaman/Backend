using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response.OnboardingSlides;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.DataAccess.Specifications.OnboardingSlides;

namespace NettoRetail.Application.CQRS.OnboardingSlides.Queries;

public record GetOnboardingSlideByIdQuery(int Id) : IRequest<OnboardingSlideResponse>;

public class GetOnboardingSlideByIdQueryHandler(IOnboardingSlideRepository slideRepository)
    : IRequestHandler<GetOnboardingSlideByIdQuery, OnboardingSlideResponse>
{
    public async Task<OnboardingSlideResponse> Handle(GetOnboardingSlideByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetOnboardingSlideByIdSpecification(request.Id);
        var slide = await slideRepository.GetAsync(spec, cancellationToken);
        if (slide is null)
            throw new NotFoundException($"Onboarding slide with ID {request.Id} not found.");

        return slide.Adapt<OnboardingSlideResponse>();
    }
}
