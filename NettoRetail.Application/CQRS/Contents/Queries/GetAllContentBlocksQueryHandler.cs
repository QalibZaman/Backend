using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Content;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Contents.Queries;

public record GetAllContentBlocksQuery(int ShopId) : IRequest<List<ContentBlockResponse>>;

public class GetAllContentBlocksQueryHandler(IContentRepository contentRepository, IMapper mapper)
    : IRequestHandler<GetAllContentBlocksQuery, List<ContentBlockResponse>>
{
    public async Task<List<ContentBlockResponse>> Handle(
        GetAllContentBlocksQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new GetContentByShopSpecification(request.ShopId);
        var mappedBlocks = (await contentRepository.GetAllBySpecAsync(specification, cancellationToken))
     .ToDictionary(x => x.SectionType, x => mapper.Map<ContentBlockResponse>(x));

        return Enum.GetValues<SectionTypeEnum>()
            .Select(sectionType => mappedBlocks.GetValueOrDefault(sectionType) 
            ?? ContentResponseFactory.CreateEmptyBlock(sectionType))
            .ToList();
    }
}
