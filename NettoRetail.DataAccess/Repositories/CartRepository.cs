using NettoRetail.DataAccess.Context;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace NettoRetail.DataAccess.Repositories
{
    public class CartRepository(AppDbContext dbContext) : BaseRepository<CartEntity>(dbContext), ICartRepository
    {
        public async Task<CartEntity?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken)
        {
             return await _dbSet.Include(c => c.Items).FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
        }

        public async Task<CartEntity?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken)
        {
            return await _dbSet.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);
        }
    }
}
