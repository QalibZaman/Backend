using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IProductDetailsRepository
{
    Task<ProductDetailsEntity?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDetailsEntity>> GetAllByPartitionKeyAsync(string partitionKey, CancellationToken cancellationToken = default);
    Task UpsertAsync(ProductDetailsEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default);
    Task ExecuteUpdatingTransactionalBatchAsync(string partitionKey, ICollection<ProductDetailsEntity> entitiesToUpsert, ICollection<string> idsToDelete, CancellationToken cancellationToken = default);
}