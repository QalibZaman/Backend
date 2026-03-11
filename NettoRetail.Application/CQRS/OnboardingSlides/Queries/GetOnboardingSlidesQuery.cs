using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Response.OnboardingSlides;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.OnboardingSlides.Queries;

public record GetOnboardingSlidesQuery() : IRequest<List<OnboardingSlideResponse>>;

public class GetOnboardingSlidesQueryHandler(IOnboardingSlideRepository slideRepository, IMapper mapper
): IRequestHandler<GetOnboardingSlidesQuery, List<OnboardingSlideResponse>>
{
    public async Task<List<OnboardingSlideResponse>> Handle(GetOnboardingSlidesQuery request, CancellationToken cancellationToken)
    {
        var slides = await slideRepository.GetAllAsync(cancellationToken);
        var orderedSlides = slides.OrderBy(x => x.Order).ToList();
        return mapper.Map<List<OnboardingSlideResponse>>(orderedSlides);
    }
}
