using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Categories.Queries;

public record GetCategoriesSectionQuery(int ShopId) : IRequest<GetCategoriesSectionResponse>;

public class GetCategoriesSectionQueryHandler(
    ICategoryRepository categoryRepository,
    IShopRepository shopRepository)
    : IRequestHandler<GetCategoriesSectionQuery, GetCategoriesSectionResponse>
{
    public async Task<GetCategoriesSectionResponse> Handle(GetCategoriesSectionQuery request, CancellationToken cancellationToken)
    {
        var shop = await shopRepository.GetByIdAsync(request.ShopId, cancellationToken);
        if (shop is null)
        {
            throw new NotFoundException($"Shop with id {request.ShopId} not found");
        }

        var availableSpec = new SpecificationBuilder<CategoryEntity>()
            .Add<GetVisibleLevelOneCategoriesByShopIdSpecification>(request.ShopId)
            .AddOrderBy(c => c.Name)
            .Build();

        var availableCategories = (await categoryRepository.GetAllBySpecAsync(availableSpec, cancellationToken)).ToList();

        if (availableCategories.Count == 0)
        {
            throw new NotFoundException(
                "Your shop doesn't have any categories yet. Please, add categories to continue.");
        }

        var selectedSpec = new SpecificationBuilder<CategoryEntity>()
            .Add<GetVisibleLevelOneCategoriesByShopIdSpecification>(request.ShopId)
            .AddCriteria(c => c.CategoriesSectionOrder != null)
            .AddOrderBy(c => c.CategoriesSectionOrder!)
            .Build();

        var selectedCategories = (await categoryRepository.GetAllBySpecAsync(selectedSpec, cancellationToken)).ToList();

        const int DefaultSelectionCount = 6;

        var selectedItems = selectedCategories.Count == 0
            ? BuildDefaultSelection(availableCategories, DefaultSelectionCount)
            : selectedCategories
                .OrderBy(c => c.CategoriesSectionOrder)
                .Select(c => new CategorySectionItemResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    DisplayOrder = c.CategoriesSectionOrder ?? 0
                })
                .ToList();

        var dto = new GetCategoriesSectionResponse
        {
            ShopId = shop.Id,
            Visibility = shop.CategoriesSectionVisibility,
            Name = shop.CategoriesSectionName ?? string.Empty,
            SelectedCategories = selectedItems,
            AvailableCategoriesCount = availableCategories.Count
        };

        return dto;
    }

    private static IReadOnlyList<CategorySectionItemResponse> BuildDefaultSelection(
        List<CategoryEntity> availableCategories,
        int selectionCount)
    {
        return availableCategories
            .OrderBy(c => c.Name)
            .Take(selectionCount)
            .Select((category, index) => new CategorySectionItemResponse
            {
                Id = category.Id,
                Name = category.Name,
                DisplayOrder = index
            })
            .ToList();
    }
}
