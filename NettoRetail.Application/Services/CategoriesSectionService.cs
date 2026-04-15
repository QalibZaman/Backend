using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
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
        var shop = await GetShopOrThrowAsync(shopId, cancellationToken);

        var availableCategories = await GetAvailableCategoriesAsync(shopId, cancellationToken);
        var selectedCategories = await GetSelectedVisibleCategoriesAsync(shopId, cancellationToken);

        var selectedItems = selectedCategories.Count == 0
            ? BuildDefaultSelection(availableCategories)
            : MapToSectionItems(selectedCategories);

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
        ValidateCategorySelection(categoryIds);

        var shop = await GetShopOrThrowAsync(shopId, cancellationToken);
        var resolvedSelection = await ResolveRequestedSelectionAsync(shopId, categoryIds, cancellationToken);

        if (resolvedSelection.Count == 0)
        {
            throw new BadRequestException(_options.AtLeastOneCategoryErrorMessage);
        }

        var currentSelectionSpec = new SpecificationBuilder<CategoryEntity>()
            .Add<GetCategoriesByShopIdSpecification>(shopId)
            .AddCriteria(c => c.CategoriesSectionOrder != null)
            .Build();

        var currentSelection = (await _categoryRepository.GetAllBySpecAsync(currentSelectionSpec, cancellationToken)).ToList();

        using var transaction = _unitOfWork.BeginTransaction();

        ApplyCategoriesSectionOrder(currentSelection, resolvedSelection);

        shop.CategoriesSectionName = name.Trim();
        shop.CategoriesSectionVisibility = visibility;

        await _shopRepository.CommitAsync(cancellationToken);

        transaction.Commit();
    }

    private void ValidateCategorySelection(IReadOnlyList<int> categoryIds)
    {
        if (categoryIds.Count < 1)
        {
            throw new BadRequestException(_options.AtLeastOneCategoryErrorMessage);
        }

        if (categoryIds.Count > _options.MaxSelectionCountValue)
        {
            throw new BadRequestException(_options.MaxSelectionErrorMessage);
        }

        if (categoryIds.Distinct().Count() != categoryIds.Count)
        {
            throw new BadRequestException(_options.DuplicateCategoriesErrorMessage);
        }

        if (categoryIds.Any(categoryId => categoryId <= 0))
        {
            throw new BadRequestException(_options.InvalidCategoryIdsErrorMessage);
        }
    }

    private async Task<List<CategoryEntity>> ResolveRequestedSelectionAsync(
        int shopId,
        IReadOnlyList<int> categoryIds,
        CancellationToken cancellationToken)
    {
        var requestedIds = categoryIds.Distinct().ToList();
        var allRequestedSpec = new SpecificationBuilder<CategoryEntity>()
            .Add<GetCategoriesByIdsSpecification>(requestedIds)
            .Build();

        var allRequestedCategories = (await _categoryRepository.GetAllBySpecAsync(allRequestedSpec, cancellationToken)).ToList();
        var categoriesInShop = allRequestedCategories
            .Where(category => category.ShopId == shopId)
            .ToDictionary(category => category.Id);

        var hasInvalidCategory = allRequestedCategories.Any(category =>
            category.ShopId != shopId || !IsSelectableCategory(category));

        if (hasInvalidCategory)
        {
            throw new BadRequestException(_options.InvalidCategoryIdsErrorMessage);
        }

        var resolvedSelection = new List<CategoryEntity>(categoryIds.Count);
        foreach (var categoryId in categoryIds)
        {
            if (!categoriesInShop.TryGetValue(categoryId, out var category))
            {
                continue;
            }

            resolvedSelection.Add(category);
        }

        return resolvedSelection;
    }

    private async Task<List<CategoryEntity>> GetAvailableCategoriesAsync(int shopId, CancellationToken cancellationToken)
    {
        var availableSpec = new SpecificationBuilder<CategoryEntity>()
            .Add<GetVisibleLevelOneCategoriesByShopIdSpecification>(shopId)
            .AddOrderBy(c => c.Name)
            .Build();

        return (await _categoryRepository.GetAllBySpecAsync(availableSpec, cancellationToken)).ToList();
    }

    private async Task<List<CategoryEntity>> GetSelectedVisibleCategoriesAsync(int shopId, CancellationToken cancellationToken)
    {
        var selectedSpec = new SpecificationBuilder<CategoryEntity>()
            .Add<GetVisibleLevelOneCategoriesByShopIdSpecification>(shopId)
            .AddCriteria(c => c.CategoriesSectionOrder != null)
            .AddOrderBy(c => c.CategoriesSectionOrder!)
            .Build();

        return (await _categoryRepository.GetAllBySpecAsync(selectedSpec, cancellationToken)).ToList();
    }

    private static IReadOnlyList<CategorySectionItemResponse> MapToSectionItems(IEnumerable<CategoryEntity> categories)
    {
        return categories
            .Select(c => new CategorySectionItemResponse
            {
                Id = c.Id,
                Name = c.Name,
                DisplayOrder = c.CategoriesSectionOrder ?? 0
            })
            .ToList();
    }

    private async Task<ShopEntity> GetShopOrThrowAsync(int shopId, CancellationToken cancellationToken)
    {
        var shop = await _shopRepository.GetByIdAsync(shopId, cancellationToken);
        if (shop is null)
        {
            throw new NotFoundException(string.Format(HandlerConstants.Shop.ShopNotFoundMessage, shopId));
        }

        return shop;
    }

    private static bool IsSelectableCategory(CategoryEntity category)
    {
        return category.Level == 1 && category.Visibility;
    }

    private static void ApplyCategoriesSectionOrder(
    List<CategoryEntity> currentSelection,
    List<CategoryEntity> resolvedSelection)
    {
        foreach (var category in currentSelection)
        {
            category.CategoriesSectionOrder = null;
        }

        for (var index = 0; index < resolvedSelection.Count; index++)
        {
            resolvedSelection[index].CategoriesSectionOrder = index;
        }
    }

    private IReadOnlyList<CategorySectionItemResponse> BuildDefaultSelection(List<CategoryEntity> availableCategories)
    {
        return availableCategories
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
