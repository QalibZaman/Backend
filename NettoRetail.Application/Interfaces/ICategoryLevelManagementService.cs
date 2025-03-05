using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Interfaces;

public interface ICategoryLevelManagementService
{
    Task SetUpCategoryLevel(CategoryEntity category, CancellationToken cancellationToken);
    Task UpdateCategoryLevel(CategoryEntity category, CancellationToken cancellationToken);
}
