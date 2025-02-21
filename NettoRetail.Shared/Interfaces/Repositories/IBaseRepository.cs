using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IBaseRepository<TEntity>
    where TEntity : class
{
    Task<TEntity> AddAsync(TEntity model, CancellationToken cancellationToken);
    Task<TEntity?> GetAsync(Specification<TEntity> expression, CancellationToken cancellationToken);
}