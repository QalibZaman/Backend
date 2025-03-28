using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IBaseRepository<TEntity>
    where TEntity : class
{
    Task<TEntity> AddAsync(TEntity model, CancellationToken cancellationToken);
    Task<TEntity?> GetAsync(Specification<TEntity> expression, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetManyBySpecAsync(Specification<TEntity> specification, CancellationToken cancellationToken);
    Task<int> GetAmountAsync(CancellationToken cancellationToken);
    Task DeleteAsync(Specification<TEntity> specification, CancellationToken cancellationToken);
    Task DeleteManyBySpecAsync(Specification<TEntity> specification, CancellationToken cancellationToken);
    Task UpdateAsync(TEntity model, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetAllBySpecAsync(Specification<TEntity> specification, CancellationToken cancellationToken);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    Task AddBulkAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    Task<int> GetAmountBySpecAsync(Specification<TEntity> specification, CancellationToken cancellationToken);
}