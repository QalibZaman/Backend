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

public record SearchCategoriesForSectionQuery(
    int ShopId,
    string? Query,
    IReadOnlyCollection<int> ExcludeIds,
    PaginationRequest PageModel)
    : IRequest<PaginatedResponse<CategorySummaryResponse>>;

public class SearchCategoriesForSectionQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<SearchCategoriesForSectionQuery, PaginatedResponse<CategorySummaryResponse>>
{
    public async Task<PaginatedResponse<CategorySummaryResponse>> Handle(
        SearchCategoriesForSectionQuery request,
        CancellationToken cancellationToken)
    {
        var searchTerm = request.Query?.Trim();
        var skip = PaginationHelper.CalculateSkip(request.PageModel.PageNumber, request.PageModel.PageSize);

        var builder = new SpecificationBuilder<CategoryEntity>()
            .Add<GetVisibleLevelOneCategoriesByShopIdSpecification>(request.ShopId)
            .AddCriteria(c => c.CategoriesSectionOrder == null)
            .AddOrderBy(c => c.Name)
            .Skip(skip)
            .Take(request.PageModel.PageSize);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            builder.Add<GetCategoriesByPartialNameSpecification>(searchTerm);
        }

        if (request.ExcludeIds.Count > 0)
        {
            builder.AddCriteria(c => !request.ExcludeIds.Contains(c.Id));
        }

        var specification = builder.Build();

        var (items, totalAmount) = await categoryRepository.GetAllPaginatedAsync(specification, cancellationToken);

        var responseItems = items
            .Select(category => new CategorySummaryResponse
            {
                Id = category.Id,
                Name = category.Name
            })
            .ToList();

        return new PaginatedResponse<CategorySummaryResponse>
        {
            Items = responseItems,
            TotalAmount = totalAmount
        };
    }
}
