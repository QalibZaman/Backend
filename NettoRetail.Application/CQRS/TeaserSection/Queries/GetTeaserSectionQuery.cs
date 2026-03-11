using MediatR;
using MapsterMapper;
using NettoRetail.Application.Responses.TeaserSection;

namespace NettoRetail.Application.CQRS.TeaserSection.Queries;

public record GetTeaserSectionQuery(int ShopId) : IRequest<TeaserSectionResponse>;

public class GetTeaserSectionQueryHandler(
    ITeaserSectionRepository teaserSectionRepository,
    IMapper mapper
) : IRequestHandler<GetTeaserSectionQuery, TeaserSectionResponse>
{
    public async Task<TeaserSectionResponse?> Handle(GetTeaserSectionQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetTeaserSectionByShopIdSpecification(request.ShopId);
        var teaser = await teaserSectionRepository.GetAsync(spec, cancellationToken);

        if (teaser == null)
            return null;

        return mapper.Map<TeaserSectionResponse>(teaser);
    }
}
