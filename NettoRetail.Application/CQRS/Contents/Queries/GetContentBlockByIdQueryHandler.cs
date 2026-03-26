using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Content;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Contents.Queries;

public record GetContentBlockByIdQuery(int Id, int ShopId)
    : IRequest<ContentBlockResponse>;

public class GetContentBlockByIdQueryHandler(IContentRepository contentRepo, IMapper mapper)
    : IRequestHandler<GetContentBlockByIdQuery, ContentBlockResponse>
{
    public async Task<ContentBlockResponse> Handle(GetContentBlockByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetContentBlockByShopIdSpecification(request.Id, request.ShopId);
        var contentBlock = await contentRepo.GetAsync(spec, cancellationToken) ?? throw new NotFoundException("Content not found");

        return mapper.Map<ContentBlockResponse>(contentBlock);
    }
}
