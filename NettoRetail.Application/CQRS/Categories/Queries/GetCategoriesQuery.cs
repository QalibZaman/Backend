using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Request.Category;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Categories.Queries;

public record GetCategoriesQuery(GetCategoriesRequest Model, int ShopId) : IRequest<GetCategoriesResponse>;

public class GetCategoriesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
    : IRequestHandler<GetCategoriesQuery, GetCategoriesResponse>
{
    public async Task<GetCategoriesResponse> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var model = request.Model;
        var skip = PaginationHelper.CalculateSkip(model.PageNumber, model.PageSize);

        var builder = new SpecificationBuilder<CategoryEntity>()
            .Add<GetCategoriesByShopIdSpecification>(request.ShopId)
            .AddInclude(c => c.ParentCategory!)
            .Skip(skip)
            .Take(model.PageSize);

        if (!string.IsNullOrWhiteSpace(model.SearchTerm))
        {
            builder.Add<GetCategoriesByPartialNameSpecification>(model.SearchTerm);
        }

        ApplySorting(builder, model.SortField, model.SortDirection);

        var specification = builder.Build();

        var (items, totalAmount) = await categoryRepository.GetAllPaginatedAsync(specification, cancellationToken);

        var responseCollection = mapper.Map<List<CategoryResponse>>(items);

        return new GetCategoriesResponse
        {
            Categories = responseCollection,
            TotalAmount = totalAmount
        };
    }

    private static void ApplySorting(
        SpecificationBuilder<CategoryEntity> builder,
        CategorySortFieldEnum? sortField,
        SortDirectionEnum? sortDirection)
    {
        if (sortField is null || sortDirection is null)
        {
            builder.AddOrderBy(c => c.CreatedAt, isDesc: true);
            return;
        }

        var isDesc = sortDirection == SortDirectionEnum.Descending;

        switch (sortField)
        {
            case CategorySortFieldEnum.Name:
                builder.AddOrderBy(c => c.Name, isDesc);
                break;

            case CategorySortFieldEnum.ParentCategory:
                builder.AddOrderBy(c => c.ParentCategory != null ? c.ParentCategory.Name : string.Empty, isDesc);
                break;

            case CategorySortFieldEnum.ItemsAmount:
                builder.AddOrderBy(c => c.Id, isDesc);
                break;
        }
    }
}