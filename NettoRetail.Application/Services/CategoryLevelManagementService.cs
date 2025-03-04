using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.Services
{
    public class CategoryLevelManagementService : ICategoryLevelManagementService
    {
        private ICategoryRepository _categoryRepository;
        private LevelOptions _levelOptions;

        public CategoryLevelManagementService(ICategoryRepository categoryRepository, IOptions<LevelOptions> levelOptions)
        {
            _categoryRepository = categoryRepository;
            _levelOptions = levelOptions.Value;
        }

        public async Task SetUpCategoryLevel(CategoryEntity category, CancellationToken cancellationToken)
        {
            if (!category.ParentId.HasValue)
            {
                category.Level = _levelOptions.MinLevel;
                return;
            }

            var specification = new GetCategoryByIdSpecification(category.ParentId);

            var parentCategory = await _categoryRepository.GetAsync(specification, cancellationToken);

            if (parentCategory is not null)
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

           var childCategories = await _categoryRepository.GetAllNested(specification, cancellationToken);

           foreach (var child in childCategories)
           {
               await UpdateCategoryLevel(child, cancellationToken);
           }
        }
    }
}
