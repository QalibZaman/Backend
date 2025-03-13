using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Categories.Queries;

public record GetCategoriesQuery(int PageNumber, int PageSize) : IRequest<GetCategoriesResponse>;

public class GetCategoriesQueryHandler(ICategoryRepository categoryRepository) : IRequestHandler<GetCategoriesQuery, GetCategoriesResponse>
{
    public async Task<GetCategoriesResponse> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<CategoryEntity>();
        var getSpecification = specificationBuilder.Skip(request.PageNumber * request.PageSize).Take(request.PageSize).Build().AddInclude(p => p.InventoryItems);

        var categories = await categoryRepository.GetAllPaginatedAsync(getSpecification, cancellationToken);

        var totalAmount = await categoryRepository.GetAmountAsync(cancellationToken);

        var responseCollection = categories.Adapt<List<CategoryResponse>>();

        var responeModel = new GetCategoriesResponse { Categories = responseCollection, TotalAmount = totalAmount };

        return responeModel;
    }
}
