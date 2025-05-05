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
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken);
    Task DeleteManyBySpecAsync(Specification<TEntity> specification, CancellationToken cancellationToken);
    Task<TEntity> UpdateAsync(TEntity model, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetAllBySpecAsync(Specification<TEntity> specification, CancellationToken cancellationToken);
    Task<bool> Exists(Specification<TEntity> specification, CancellationToken cancellationToken);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    Task AddBulkAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    Task<int> GetAmountBySpecAsync(Specification<TEntity> specification, CancellationToken cancellationToken);
    Task<(ICollection<TEntity> Items, int TotalCount)> GetAllPaginatedAsync(Specification<TEntity> specification, CancellationToken cancellationToken);
    Task UpdateBulkAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    Task<TEntity> GetWithRelatedDataAsync(Specification<TEntity> specification, CancellationToken cancellationToken);
}