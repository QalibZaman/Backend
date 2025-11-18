using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class ItemComparisonRepository(AppDbContext dbContext)
    : BaseRepository<ItemComparisonEntity>(dbContext), IItemComparisonRepository
{
    public async Task<ItemComparisonEntity> AddComparisonAsync(
        Guid itemId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var comparison = new ItemComparisonEntity()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ComparisonItems = new List<ItemComparisonItemEntity>()
        };

        comparison.AddItem(itemId);
        await _dbSet.AddAsync(comparison);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return comparison;
    }

    public async Task<ItemComparisonEntity> DeleteItemAsync(
        ItemComparisonEntity comparison,
        ItemComparisonItemEntity link,
        CancellationToken cancellationToken)
    {
        comparison.ComparisonItems.Remove(link);

        if (comparison.ComparisonItems.Count == 0)
        {
            _dbSet.Remove(comparison);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return comparison;
    }
}
