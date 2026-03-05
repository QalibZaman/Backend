using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IInventoryItemRepository : IBaseRepository<InventoryItemEntity>
{
    Task<int> GetFilteredAmountAsync(Specification<InventoryItemEntity> specification, CancellationToken cancellationToken);
    Task<List<InventoryItemEntity>> GetAllBySortedAsync(Specification<InventoryItemEntity> specification, CancellationToken cancellationToken);
    Task<HashSet<string>> GetAllUpcHashAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<InventoryItemEntity> GetRangeByIdsAsync(Specification<InventoryItemEntity> specification, CancellationToken cancellationToken);
    Task<int> GetImagesAmountAsync(Specification<InventoryItemEntity> specification, CancellationToken cancellationToken);
}
