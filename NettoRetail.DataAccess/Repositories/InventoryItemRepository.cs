using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Specifications;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace NettoRetail.DataAccess.Repositories;

public class InventoryItemRepository(AppDbContext dbContext, IOptions<PaginationOptions> paginationOptions)
    : BaseRepository<InventoryItemEntity>(dbContext), IInventoryItemRepository
{
    private PaginationOptions _paginationDefaults = paginationOptions.Value;

    public async Task<int> GetFilteredAmountAsync(Specification<InventoryItemEntity> specification, CancellationToken cancellationToken)
    {
        var result = await _dbSet.Where(specification.Criteria).CountAsync(cancellationToken);
        return result;
    }

    public async Task<ICollection<InventoryItemEntity>> GetPaginatedAsync(Specification<InventoryItemEntity> specification, CancellationToken cancellationToken)
    {
        var query = _dbSet.Where(specification.Criteria);
        bool firstOrdering = true;

        foreach (var orderBy in specification.Orderings)
        {
            if (firstOrdering)
            {
                query = orderBy.IsDesc
                    ? query.OrderByDescending(orderBy.Expr)
                    : query.OrderBy(orderBy.Expr);

                firstOrdering = false;
            } 
            else
            {
                query = orderBy.IsDesc
                    ? ((IOrderedQueryable<InventoryItemEntity>)query).ThenByDescending(orderBy.Expr)
                    : ((IOrderedQueryable<InventoryItemEntity>)query).ThenBy(orderBy.Expr);
            }
        }

        var result = await query.Skip(specification.Skip ?? 0)
            .Take(specification.Take ?? _paginationDefaults.PageSize)
            .ToListAsync(cancellationToken);

        return result;
    }
}