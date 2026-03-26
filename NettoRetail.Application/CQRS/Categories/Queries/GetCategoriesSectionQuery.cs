using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Category;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.Application.CQRS.Categories.Queries;

public record GetCategoriesSectionQuery(int ShopId) : IRequest<GetCategoriesSectionResponse>;

public class GetCategoriesSectionQueryHandler(
    ICategoriesSectionService categoriesSectionService)
    : IRequestHandler<GetCategoriesSectionQuery, GetCategoriesSectionResponse>
{
    public async Task<GetCategoriesSectionResponse> Handle(GetCategoriesSectionQuery request, CancellationToken cancellationToken)
    {
        return await categoriesSectionService.GetCategoriesSectionAsync(request.ShopId, cancellationToken);
    }
}
