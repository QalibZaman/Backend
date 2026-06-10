using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Content;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Contents.Queries;

public record GetContentBlockBySlugQuery(string Slug) : IRequest<ContentBlockResponse>;

public class GetContentBlockBySlugQueryHandler(IContentRepository contentRepo, IMapper mapper)
    : IRequestHandler<GetContentBlockBySlugQuery, ContentBlockResponse>
{
    public async Task<ContentBlockResponse> Handle(GetContentBlockBySlugQuery request, CancellationToken cancellationToken)
    {
        var (shopId, sectionType) = ContentSlugParser.ParseContentSlug(request.Slug);
        if (shopId == default || sectionType == default)
            throw new NotFoundException($"Content block with slug '{request.Slug}' was not found");
        
        var spec = new GetContentByShopAndSectionSpecification(shopId, sectionType);
        var contentBlock = await contentRepo.GetAsync(spec, cancellationToken);
        if (contentBlock == null)
            throw new NotFoundException($"Content block with slug '{request.Slug}' was not found.");


        return mapper.Map<ContentBlockResponse>(contentBlock);
    }
}
