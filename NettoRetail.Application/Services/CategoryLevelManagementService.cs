using MapsterMapper;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services
{
    public class CategoryLevelManagementService(
        ICategoryRepository categoryRepository,
        IOptions<LevelOptions> levelOptions,
        IMapper mapper) : ICategoryLevelManagementService
    {
        private readonly LevelOptions _levelOptions = levelOptions.Value;
        private ICategoryRepository _categoryRepository = categoryRepository;

        public async Task SetUpCategoryLevel(CategoryEntity category, CancellationToken cancellationToken)
        {
            if (!category.ParentId.HasValue)
            {
                category.Level = _levelOptions.MinLevel;
                return;
            }

            var specification = new GetCategoryByIdSpecification(category.ParentId);

            var parentCategory = await categoryRepository.GetAsync(specification, cancellationToken);

            if (parentCategory != null)
            {
                category.Level = parentCategory.Level + 1;

                if (category.Level > _levelOptions.MaxLevel)
                {
                    throw new BadRequestException($"Category level exceeds maximum allowed level of {_levelOptions.MaxLevel}.");
                }
            }
        }

        public async Task UpdateCategoryLevel(CategoryEntity category, CancellationToken cancellationToken)
        {
            await SetUpCategoryLevel(category, cancellationToken);

            await _categoryRepository.UpdateAsync(category, cancellationToken);

            var specification = new GetCategoriesOfParentByIdSpecification(category.Id);

            var childCategories = await _categoryRepository.GetAllNestedAsync(specification, cancellationToken);

            foreach (var child in childCategories)
            {
                await UpdateCategoryLevel(child, cancellationToken);
            }
        }

        public async Task<PaginatedResponse<CategoryResponse>> GetShopCategoriesBySearchAsync(
            string query, int shopId, PaginationRequest pageModel, CancellationToken cancellationToken)
        {
            var categorySpecBuilder = new SpecificationBuilder<CategoryEntity>();
            categorySpecBuilder
                .Add<GetCategoriesByPartialNameSpecification>(query)
                .Add<GetCategoriesByShopIdSpecification>(shopId);

            var categorySpec = categorySpecBuilder.Build();

            var categories = await _categoryRepository.GetAllBySpecAsync(categorySpec, cancellationToken);
            var pagedCategories = categories
                .Skip(PaginationHelper.CalculateSkip(pageModel.PageNumber, pageModel.PageSize))
                .Take(pageModel.PageSize)
                .ToList();

            var categoryResponses = mapper.Map<List<CategoryResponse>>(pagedCategories);

            var response = new PaginatedResponse<CategoryResponse>
            {
                Items = categoryResponses,
                TotalAmount = categories.Count(),
            };
            return response;
        }

        public async Task<CategoryEntity> GetCategoryByNameWithCacheAsync(
        Dictionary<string, CategoryEntity> cache,
        string categoryName,
        int shopId,
        CancellationToken cancellationToken)
        {
            if (cache.TryGetValue(categoryName, out var cached))
                return cached;

            var spec = new GetCategoriesByNameOrUrlSpecification(categoryName, null, shopId);
            var category = await categoryRepository.GetAsync(spec, cancellationToken)
                ?? throw new NotFoundException($"Category '{categoryName}' not found.");
            cache[categoryName] = category;
            return category;
        }
    }
}
