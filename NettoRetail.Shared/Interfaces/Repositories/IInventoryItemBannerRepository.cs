using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories
{
    public interface IInventoryItemBannerRepository : IBaseRepository<InventoryItemBannerEntity>
    {
        Task<ICollection<InventoryItemBannerEntity>> GetByInventoryItemIdAsync(Guid inventoryItemId, Specification<InventoryItemBannerEntity> specification, CancellationToken cancellationToken);
        Task<InventoryItemBannerEntity?> GetByIdAsync(Guid bannerId, CancellationToken cancellationToken);
        Task<int> GetMaxOrderIndexAsync(Guid inventoryItemId, CancellationToken cancellationToken);
    }
}
