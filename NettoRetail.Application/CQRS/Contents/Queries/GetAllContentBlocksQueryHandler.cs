using Mapster;
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
        var contentBlock = await contentRepo.GetAllBySpecAsync(spec, cancellationToken);
        if (!contentBlock.Any())
        {
            throw new NotFoundException($"Contents not found with shop id {request.ShopId}.");
        }

        return contentBlock.Adapt<List<ContentBlockResponse>>();
    }
}
