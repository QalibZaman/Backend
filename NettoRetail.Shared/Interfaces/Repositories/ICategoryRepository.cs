using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface ICategoryRepository : IBaseRepository<CategoryEntity>
{
    Task<ICollection<CategoryEntity>> GetAllNested(Specification<CategoryEntity> specification, CancellationToken cancellationToken);
    Task<int> GetAmount(CancellationToken cancellationToken);
    Task<ICollection<CategoryEntity>> GetAllPaginated(Specification<CategoryEntity> specification, CancellationToken cancellationToken);
}
