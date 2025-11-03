using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Interfaces;

public interface ICategoryLevelManagementService
{
    Task SetUpCategoryLevel(CategoryEntity category, CancellationToken cancellationToken);
    Task UpdateCategoryLevel(CategoryEntity category, CancellationToken cancellationToken);

    Task<CategoryEntity> GetOrAddCategoryAsync(
      ICategoryRepository repository,
      Dictionary<string, CategoryEntity> cache,
      string categoryName,
      int shopId,
      CancellationToken cancellationToken);
}
