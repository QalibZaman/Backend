using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.ICategoryService;

public class CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var specification = new GetCategoryByIdSpecification(id);

        var category = await categoryRepository.GetAsync(specification, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException($"There is no category with Id: {id}");
        }

        await categoryRepository.DeleteAsync(specification, cancellationToken);
        logger.LogInformation("Category deleted successfully. Id: {CategoryId}", id);
    }
}
