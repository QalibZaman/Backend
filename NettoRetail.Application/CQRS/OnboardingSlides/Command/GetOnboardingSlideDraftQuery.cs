using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response.OnboardingSlides;
using NettoRetail.DataAccess.Specifications.OnboardingSlides;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.OnboardingSlides.Queries;

public record GetOnboardingSlideDraftQuery(int? SlideId, Guid UserId) : IRequest<OnboardingSlideDraftResponse>;

public class GetOnboardingSlideDraftQueryHandler(
    IOnboardingSlideDraftRepository draftRepository
) : IRequestHandler<GetOnboardingSlideDraftQuery, OnboardingSlideDraftResponse>
{
    public async Task<OnboardingSlideDraftResponse> Handle(GetOnboardingSlideDraftQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetOnboardingSlideDraftByUserAndSlideIdSpecification(request.UserId, request.SlideId);
        var draft = await draftRepository.GetAsync(spec, cancellationToken);

        if (draft is null)
            throw new NotFoundException("Draft not found.");

        return draft.Adapt<OnboardingSlideDraftResponse>();
    }
}
