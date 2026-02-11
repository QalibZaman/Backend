using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class OrderStatusHistoryRepository(AppDbContext dbContext)
    : BaseRepository<OrderStatusHistoryEntity>(dbContext), IOrderStatusHistoryRepository
{
    public async Task<IEnumerable<OrderStatusHistoryEntity>> GetOrderHistoryIncludingAccount(int id)
    {
        var history = await _dbSet.Where(x => x.OrderId == id)
            .Include(x => x.UpdatedBy)
            .ToListAsync();

        return history;
    }
}
