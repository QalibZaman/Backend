using MediatR;
using MapsterMapper;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Responses.TeaserSection;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.CQRS.TeaserSection.Queries;

public record GetTeaserSectionQuery(int ShopId) : IRequest<TeaserSectionResponse>;

public class GetTeaserSectionQueryHandler(
    ITeaserSectionRepository teaserSectionRepository,
    IMapper mapper
) : IRequestHandler<GetTeaserSectionQuery, TeaserSectionResponse>
{
    public async Task<TeaserSectionResponse> Handle(GetTeaserSectionQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetTeaserSectionByShopIdSpecification(request.ShopId);
        var teaser = await teaserSectionRepository.GetAsync(spec, cancellationToken);

        return teaser == null ? throw new NotFoundException(string.Format(ValidationErrorMessages.TeaserSection.TeaserSectionNotFoundErrorMessage, request.ShopId)) : mapper.Map<TeaserSectionResponse>(teaser);
    }
}
