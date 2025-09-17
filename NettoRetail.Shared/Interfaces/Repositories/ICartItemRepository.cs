using NettoRetail.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Shared.Interfaces.Repositories
{
    public interface ICartItemRepository : IBaseRepository<CartItemEntity>
    {
        Task<CartItemEntity?> GetByCartAndInventoryAsync(Guid cartId, Guid inventoryItemId, string? size, CancellationToken ct);
    }
}
