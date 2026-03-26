using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services;

public class CategoriesSectionService : ICategoriesSectionService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CategoriesSectionOptions _options;

    public CategoriesSectionService(
        ICategoryRepository categoryRepository,
        IShopRepository shopRepository,
        IUnitOfWork unitOfWork,
        IOptions<CategoriesSectionOptions> options)
    {
        _categoryRepository = categoryRepository;
        _shopRepository = shopRepository;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task<GetCategoriesSectionResponse> GetCategoriesSectionAsync(int shopId, CancellationToken cancellationToken)
    {
        var shop = await _shopRepository.GetByIdAsync(shopId, cancellationToken);
        if (shop is null)
        {
            throw new NotFoundException($"Shop with id {shopId} not found");
        }

        var availableSpec = new SpecificationBuilder<CategoryEntity>()
            .Add<GetVisibleLevelOneCategoriesByShopIdSpecification>(shopId)
            .AddOrderBy(c => c.Name)
            .Build();

        var availableCategories = (await _categoryRepository.GetAllBySpecAsync(availableSpec, cancellationToken)).ToList();

        var selectedSpec = new SpecificationBuilder<CategoryEntity>()
            .Add<GetVisibleLevelOneCategoriesByShopIdSpecification>(shopId)
            .AddCriteria(c => c.CategoriesSectionOrder != null)
            .AddOrderBy(c => c.CategoriesSectionOrder!)
            .Build();

        var selectedCategories = (await _categoryRepository.GetAllBySpecAsync(selectedSpec, cancellationToken)).ToList();

        var selectedItems = selectedCategories.Count == 0
            ? BuildDefaultSelection(availableCategories)
            : selectedCategories
            .Select(c => new CategorySectionItemResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    DisplayOrder = c.CategoriesSectionOrder ?? 0
                })
                .ToList();

        return new GetCategoriesSectionResponse
        {
            ShopId = shop.Id,
            Visibility = shop.CategoriesSectionVisibility,
            Name = shop.CategoriesSectionName ?? string.Empty,
            SelectedCategories = selectedItems,
            AvailableCategoriesCount = availableCategories.Count
        };
    }

    public async Task UpdateCategoriesSectionAsync(int shopId, string name, bool visibility, IReadOnlyList<int> categoryIds, CancellationToken cancellationToken)
    {
        if (categoryIds.Count < 1)
        {
            throw new BadRequestException(_options.AtLeastOneCategoryErrorMessage);
        }

        if (categoryIds.Count > _options.MaxSelectionCountValue)
        {
            throw new BadRequestException(_options.MaxSelectionErrorMessage);
        }

        var distinctIds = categoryIds.Distinct().ToList();
        if (distinctIds.Count != categoryIds.Count)
        {
            throw new BadRequestException(_options.DuplicateCategoriesErrorMessage);
        }

        var shop = await _shopRepository.GetByIdAsync(shopId, cancellationToken);
        if (shop is null)
        {
            throw new NotFoundException($"Shop with id {shopId} not found");
        }

        var requestedCategoriesSpec = new SpecificationBuilder<CategoryEntity>()
            .Add<GetVisibleLevelOneCategoriesByShopIdSpecification>(shopId)
            .Add<GetCategoriesByIdsSpecification>(distinctIds)
            .Build();

        var requestedCategories = (await _categoryRepository.GetAllBySpecAsync(requestedCategoriesSpec, cancellationToken)).ToList();
        if (requestedCategories.Count != distinctIds.Count)
        {
            throw new BadRequestException("Selected categories must be level 1, visible, and belong to your shop.");
        }

        var currentSelectionSpec = new SpecificationBuilder<CategoryEntity>()
            .Add<GetCategoriesByShopIdSpecification>(shopId)
            .AddCriteria(c => c.CategoriesSectionOrder != null)
            .Build();

        var currentSelection = (await _categoryRepository.GetAllBySpecAsync(currentSelectionSpec, cancellationToken)).ToList();
        var requestedById = requestedCategories.ToDictionary(category => category.Id);

        using var transaction = _unitOfWork.BeginTransaction();

        foreach (var category in currentSelection)
        {
            category.CategoriesSectionOrder = null;
        }

        var categoriesToUpdate = currentSelection.ToDictionary(category => category.Id);

        for (var index = 0; index < categoryIds.Count; index++)
        {
            var categoryId = categoryIds[index];
            var category = requestedById[categoryId];
            category.CategoriesSectionOrder = index;
            categoriesToUpdate[category.Id] = category;
        }

        shop.CategoriesSectionName = name.Trim();
        shop.CategoriesSectionVisibility = visibility;

        await _categoryRepository.UpdateManyAsync(categoriesToUpdate.Values, cancellationToken);
        await _shopRepository.UpdateAsync(shop, cancellationToken);

        transaction.Commit();
    }

    private IReadOnlyList<CategorySectionItemResponse> BuildDefaultSelection(List<CategoryEntity> availableCategories)
    {
        return availableCategories
            .OrderBy(c => c.Name)
            .Take(_options.DefaultSelectionCountValue)
            .Select((category, index) => new CategorySectionItemResponse
            {
                Id = category.Id,
                Name = category.Name,
                DisplayOrder = index
            })
            .ToList();
    }
}
