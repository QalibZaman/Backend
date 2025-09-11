using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories
{
    public class InventoryItemBannerRepository(AppDbContext dbContext) : BaseRepository<InventoryItemBannerEntity>(dbContext), IInventoryItemBannerRepository
    {
        public async Task<ICollection<InventoryItemBannerEntity>> GetByInventoryItemIdAsync(
            Guid inventoryItemId,
            Specification<InventoryItemBannerEntity> specification,
            CancellationToken cancellationToken)
        {
            IQueryable<InventoryItemBannerEntity> query = _dbSet.Where(b => b.InventoryItemId == inventoryItemId);

            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);

            foreach (var include in specification.Includes)
                query = query.Include(include);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<InventoryItemBannerEntity?> GetByIdAsync(Guid bannerId, CancellationToken cancellationToken)
        {
            return await _dbSet.FirstOrDefaultAsync(b => b.Id == bannerId, cancellationToken);
        }

        public async Task<int> GetMaxOrderIndexAsync(Guid inventoryItemId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(b => b.InventoryItemId == inventoryItemId)
                .MaxAsync(b => (int?)b.OrderIndex, cancellationToken) ?? 0;
        }
    }
}
