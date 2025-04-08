using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
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

    public async Task<HashSet<long>> GetAllUpcHashAsync(CancellationToken cancellationToken)
    {
        var result = await _dbSet.Select(x => x.ProductUpc).ToHashSetAsync(cancellationToken);

        return result;
    }

    public async IAsyncEnumerable<InventoryItemEntity> GetRangeByIdsAsync(Specification<InventoryItemEntity> specification, CancellationToken cancellationToken)
    {
        var queryable = _dbSet.Where(specification.Criteria);

        await foreach (var item in queryable.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }
}