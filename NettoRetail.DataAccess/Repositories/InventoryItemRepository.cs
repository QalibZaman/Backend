using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class InventoryItemRepository(AppDbContext dbContext)
    : BaseRepository<InventoryItemEntity>(dbContext), IInventoryItemRepository
{
    public async Task<int> GetFilteredAmountAsync(Specification<InventoryItemEntity> specification, CancellationToken cancellationToken)
    {
        var result = await _dbSet.Where(specification.Criteria).CountAsync(cancellationToken);
        return result;
    }

    public async Task<List<InventoryItemEntity>> GetAllBySortedAsync(Specification<InventoryItemEntity> specification, CancellationToken cancellationToken)
    {
        IQueryable<InventoryItemEntity> query = _dbSet.Where(specification.Criteria);
        foreach (var includeExpression in specification.Includes)
        {
            query = query.Include(includeExpression);
        }

        foreach (var (orderExpression, isDescending) in specification.Orderings)
        {
            if (isDescending)
                query = query.OrderByDescending(orderExpression);
            else
                query = query.OrderBy(orderExpression);
        }

        if (specification.Skip.HasValue)
            query = query.Skip(specification.Skip.Value);
        if (specification.Take.HasValue)
            query = query.Take(specification.Take.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<HashSet<string>> GetAllUpcHashAsync(CancellationToken cancellationToken)
    {
        var result = await _dbSet.Select(x => x.ProductUpc).ToHashSetAsync(cancellationToken);

        return result;
    }

    public async IAsyncEnumerable<InventoryItemEntity> GetRangeByIdsAsync(
        Specification<InventoryItemEntity> specification,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var queryable = _dbSet.Where(specification.Criteria);

        await foreach (var item in queryable.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }

    public async Task<int> GetImagesAmountAsync(Specification<InventoryItemEntity> specification, CancellationToken cancellationToken)
    {
        var result = await _dbSet.Where(specification.Criteria)
            .Select((iie) => iie.Images)
            .CountAsync(cancellationToken);
        return result;
    }
}
