using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;
public class RecentlyViewedRepository(AppDbContext dbContext) : BaseRepository<RecentlyViewedProductEntity>(dbContext), IRecentlyViewedRepository
{
    public async Task MergeAsync(Guid accountId, IEnumerable<RecentlyViewedProductEntity> guestItems, CancellationToken cancellationToken)
    {
        var existing = await _dbSet
            .Where(x => x.AccountId == accountId)
            .ToListAsync(cancellationToken);

        var merged = existing
            .Concat(guestItems)
            .GroupBy(x => x.InventoryItemId)
            .Select(g => g.OrderByDescending(x => x.ViewedAtUtc).First())
            .OrderByDescending(x => x.ViewedAtUtc)
            .Take(5)
            .ToList();

        _dbSet.RemoveRange(existing);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _dbSet.AddRangeAsync(merged, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
