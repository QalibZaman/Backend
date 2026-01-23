using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class OrderRepository(AppDbContext dbContext)
    : BaseRepository<OrderEntity>(dbContext), IOrderRepository
{
    public async Task<OrderEntity?> GetOrderForPrinting(Specification<OrderEntity> specification, CancellationToken cancellationToken)
    {
        IQueryable<OrderEntity> query = _dbSet;

        query = query.Where(specification.Criteria);

        foreach (var include in specification.UpdatedIncludes)
        {
            query = include(query);
        }

        var order = await query
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

        return order;
    }

    public async Task<OrderEntity?> GetOrderWithInventoryItemsAsync(Specification<OrderEntity> specification, CancellationToken cancellationToken)
    {
        var order = await _dbSet.Where(specification.Criteria)
            .Include(o => o.OrderItems)
                .ThenInclude(o => o.InventoryItem)
            .FirstOrDefaultAsync(cancellationToken);

        return order;
    }
}