using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface ICategoryRepository : IBaseRepository<CategoryEntity>
{
    Task<ICollection<CategoryEntity>> GetAllNestedAsync(Specification<CategoryEntity> specification, CancellationToken cancellationToken);
    Task<ICollection<CategoryEntity>> GetAllPaginatedAsync(Specification<CategoryEntity> specification, CancellationToken cancellationToken);
    Task<List<CategoryEntity>> GetCategorySubtreeAsync(int categoryId, CancellationToken cancellationToken);
    Task<List<CategoryEntity>> GetCategoryWithHierarchyAsync(int categoryId, CancellationToken cancellationToken);
}
