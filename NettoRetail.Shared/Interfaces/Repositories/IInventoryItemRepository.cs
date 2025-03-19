using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IInventoryItemRepository : IBaseRepository<InventoryItemEntity>
{
    public Task<ICollection<InventoryItemEntity>> GetPaginatedAsync(Specification<InventoryItemEntity> specification, CancellationToken cancellationToken);
    public Task<int> GetFilteredAmountAsync(Specification<InventoryItemEntity> specification, CancellationToken cancellationToken);
}