using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.Application.Utilities;
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
        var skip = PaginationHelper.CalculateSkip(request.Model.PageNumber, request.Model.PageSize);

        var specification = new SpecificationBuilder<CategoryEntity>()
            .Add<GetCategoriesByShopIdSpecification>(request.ShopId)
            .Skip(skip)
            .Take(request.Model.PageSize)
            .AddInclude(p => p.InventoryItems)
            .Build();

        var (items, totalAmount) = await categoryRepository.GetAllPaginatedAsync(specification, cancellationToken);

        var responseCollection = items.Adapt<List<CategoryResponse>>();

        var responeModel = new GetCategoriesResponse { Categories = responseCollection, TotalAmount = totalAmount };

        return responeModel;
    }
}
