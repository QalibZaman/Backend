using MediatR;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Content;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Contents.Queries;

public record GetContentBlockByIdQuery(int Id, int ShopId)
    : IRequest<ContentBlockResponse?>;

public class GetContentBlockByIdHandler(IContentRepository contentRepo)
    : IRequestHandler<GetContentBlockByIdQuery, ContentBlockResponse?>
{
    public async Task<ContentBlockResponse?> Handle(GetContentBlockByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetContentBlockByIdSpecification(request.Id, request.ShopId);
        var contentBlock = await contentRepo.GetAsync(spec, cancellationToken) ?? null;

        var result = new ContentBlockResponse
        {
            Id = contentBlock.Id,
            SectionType = contentBlock.SectionType,
            ContentUrl = contentBlock.ContentUrl,
            IsVisible = contentBlock.IsVisible,
            LastUpdated = contentBlock.LastUpdated,
            UpdatedBy = contentBlock.UpdatedBy
        };

        return result;
    }
}