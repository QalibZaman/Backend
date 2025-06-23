using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IInventoryItemRepository : IBaseRepository<InventoryItemEntity>
{
    Task<int> GetFilteredAmountAsync(Specification<InventoryItemEntity> specification, CancellationToken cancellationToken);
    Task<IEnumerable<InventoryItemEntity>> GetAllBySortedAsync(Specification<InventoryItemEntity> specification, CancellationToken cancellationToken);
    Task<HashSet<long>> GetAllUpcHashAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<InventoryItemEntity> GetRangeByIdsAsync(Specification<InventoryItemEntity> specification, CancellationToken cancellationToken);
}