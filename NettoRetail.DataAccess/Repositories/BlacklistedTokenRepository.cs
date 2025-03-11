using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class BlacklistedTokenRepository(AppDbContext dbContext)
    : BaseRepository<BlacklistedTokenEntity>(dbContext), IBlacklistedTokenRepository
{
    public async Task DeleteRangeAsync(IEnumerable<BlacklistedTokenEntity> entitiesToRemove, CancellationToken cancellationToken)
    {
        _dbSet.RemoveRange(entitiesToRemove);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}