using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class OrderRepository(AppDbContext dbContext)
    : BaseRepository<OrderEntity>(dbContext), IOrderRepository
{
    public async Task<OrderEntity?> GetOrderForPrinting(Specification<OrderEntity> specification, CancellationToken )
    {
        var order = await _dbSet.Where(specification.Criteria)
            .Include(o => o.Customer)
                .ThenInclude(c => c.Account)
            .Include(o => o.StatusHistory)
            .Include(o => o.OrderItems)
                .ThenInclude(o => o.InventoryItem)
            .FirstOrDefaultAsync();

        return order;
    }
}