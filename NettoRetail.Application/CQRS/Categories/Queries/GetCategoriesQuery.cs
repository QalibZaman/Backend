using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Categories.Queries;

public record GetCategoriesQuery(int? PageNumber, int? PageSize, int ShopId) : IRequest<GetCategoriesResponse>;

public class GetCategoriesQueryHandler(ICategoryRepository categoryRepository) : IRequestHandler<GetCategoriesQuery, GetCategoriesResponse>
{
    public async Task<GetCategoriesResponse> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<CategoryEntity>();
        var pageSize = request.PageSize ?? PaginationConstants.DefaultPageSize;
        var skip = request.PageNumber * pageSize ?? 0;

        var getSpecification = specificationBuilder
            .Add<GetCategoriesByShopIdSpecification>(request.ShopId)
            .Skip(skip)
            .Take(pageSize)
            .Build()
            .AddInclude(p => p.InventoryItems);

        var categories = await categoryRepository.GetAllPaginatedAsync(getSpecification, cancellationToken);
        var (items, totalAmount) = categories;

        var responseCollection = items.Adapt<List<CategoryResponse>>();

        var responeModel = new GetCategoriesResponse { Categories = responseCollection, TotalAmount = totalAmount };

        return responeModel;
    }
}
