using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories
{
    public class CartRepository(AppDbContext dbContext) : BaseRepository<CartEntity>(dbContext), ICartRepository
    {
        public async Task<CartEntity?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken)
        {
             return await _dbSet.Include(c => c.Items.Where(i => i.IsDeleted == false)).FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
        }

        public async Task<CartEntity?> GetCartWithItemsAsync(Guid customerId, CancellationToken cancellationToken)
        {
            return await _dbContext.Carts
                .AsNoTracking()
                .Include(c => c.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.InventoryItem)
                        .ThenInclude(inv => inv.Images)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
        }
    }
}
