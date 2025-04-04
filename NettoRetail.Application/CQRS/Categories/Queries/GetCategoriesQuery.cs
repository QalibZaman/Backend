using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Categories.Queries;

public record GetCategoriesQuery(PaginationRequest Model, int ShopId) : IRequest<GetCategoriesResponse>;

public class GetCategoriesQueryHandler(ICategoryRepository categoryRepository) : IRequestHandler<GetCategoriesQuery, GetCategoriesResponse>
{
    public async Task<GetCategoriesResponse> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<CategoryEntity>();
        var skip = request.Model.PageNumber * request.Model.PageSize;

        var getSpecification = specificationBuilder
            .Add<GetCategoriesByShopIdSpecification>(request.ShopId)
            .Skip(skip)
            .Take(request.Model.PageSize)
            .Build()
            .AddInclude(p => p.InventoryItems);

        var (items, totalAmount) = await categoryRepository.GetAllPaginatedAsync(getSpecification, cancellationToken);

        var responseCollection = items.Adapt<List<CategoryResponse>>();

        var responeModel = new GetCategoriesResponse { Categories = responseCollection, TotalAmount = totalAmount };

        return responeModel;
    }
}
