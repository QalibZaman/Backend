using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.ICategoryService;

public class CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var specification = new GetCategoryByIdSpecification(id);
        specification.AddInclude(c => c.InventoryItems);

        var category = await categoryRepository.GetAsync(specification, cancellationToken)
        ?? throw new NotFoundException($"There is no category with Id: {id}");

        var conflictModelSpecification = new GetCategoriesOfParentByIdSpecification(id);
        var subcategoryExists = await categoryRepository.Exists(conflictModelSpecification, cancellationToken);

        if (subcategoryExists)
        {
            logger.LogWarning("Attempt to delete category with Id {CategoryId} that has subcategories.", id);
            throw new BadRequestException($"Cannot delete category with Id {id} as it has subcategories.");
        }

        if (category.InventoryItems.Any(i => i.Status != InventoryItemStatusEnum.Unavailable))
        {
            logger.LogWarning("Attempt to delete category with Id {CategoryId} that contains products.", id);
            throw new ConflictException($"Cannot delete category with Id {id} as it contains products.");
        }

        await categoryRepository.DeleteAsync(specification, cancellationToken);
        logger.LogInformation("Category deleted successfully. Id: {CategoryId}", id);
    }
}
