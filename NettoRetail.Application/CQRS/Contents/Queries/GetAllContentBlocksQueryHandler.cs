using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Content;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Contents.Queries;

public record GetAllContentBlocksQuery(int ShopId) : IRequest<List<ContentBlockResponse>>;

public class GetAllContentBlocksQueryHandler(IContentRepository contentRepo)
    : IRequestHandler<GetAllContentBlocksQuery, List<ContentBlockResponse>>
{
    public async Task<List<ContentBlockResponse>> Handle(GetAllContentBlocksQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetContentByShopSpecification(request.ShopId);
        var blocks = await contentRepo.GetAllBySpecAsync(spec, cancellationToken);

        return blocks.Select(b => new ContentBlockResponse
        {
            Id = b.Id,
            SectionType = b.SectionType,
            ContentUrl = b.ContentUrl,
            IsVisible = b.IsVisible,
            LastUpdated = b.LastUpdated,
            UpdatedBy = b.UpdatedBy
        }).ToList();
    }
}
