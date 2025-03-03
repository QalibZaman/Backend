using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services
{
    public class CategoryLevelManagementService(ICategoryRepository categoryRepository) : ICategoryLevelManagementService
    {
        private const int MAX_LEVEL = 3;
        private const int MIN_LEVEL = 1;

        public async Task SetUpCategoryLevel(CategoryEntity category, CancellationToken cancellationToken)
        {
            if (category.ParentId == null)
            {
                category.Level = MIN_LEVEL;
            }
            else
            {
                var parentCategory = await categoryRepository.GetAsync(new GetCategoryByIdSpecification(category.ParentId), cancellationToken);

                if (parentCategory != null)
                {
                    category.Level = parentCategory.Level + 1;

                    if (category.Level > MAX_LEVEL)
                    {
                        throw new BadRequestException($"Category level exceeds maximum allowed level of {MAX_LEVEL}.");
                    }
                }
            }
        }

        public async Task UpdateCategoryLevel(CategoryEntity category, CancellationToken cancellationToken)
        {
           await SetUpCategoryLevel(category, cancellationToken);

           await categoryRepository.UpdateAsync(category, cancellationToken);

           var childCategories = await categoryRepository.GetAllNested(new GetCategoriesOfParentByIdSpecification(category.Id), cancellationToken);

           if (childCategories.Count > 0)
           {
                foreach (var child in childCategories)
                {
                    await UpdateCategoryLevel(child, cancellationToken);
                }
           }
        }
    }
}
