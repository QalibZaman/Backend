using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IItemComparisonRepository : IBaseRepository<ItemComparisonEntity>
{
    public Task<ItemComparisonEntity> AddComparisonAsync(Guid itemId, Guid userId, CancellationToken cancellationToken);
    public Task<ItemComparisonEntity> DeleteItemAsync(ItemComparisonEntity comparison, ItemComparisonItemEntity link, CancellationToken cancellationToken);
}
