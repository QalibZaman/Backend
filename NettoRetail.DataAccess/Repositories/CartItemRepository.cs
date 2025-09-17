using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.DataAccess.Repositories
{
    public class CartItemRepository : BaseRepository<CartItemEntity>, ICartItemRepository
    {
        public CartItemRepository(AppDbContext dbContext) : base(dbContext) { }

        public async Task<CartItemEntity?> GetByCartAndInventoryAsync(Guid cartId, Guid inventoryItemId, string? size, CancellationToken ct)
        {
            return await _dbSet.FirstOrDefaultAsync(
                x => x.CartId == cartId
                  && x.InventoryItemId == inventoryItemId
                  && x.Size == size,
                ct
            );
        }
    }
}
