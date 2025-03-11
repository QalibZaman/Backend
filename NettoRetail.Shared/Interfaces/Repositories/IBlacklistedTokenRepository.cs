using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IBlacklistedTokenRepository : IBaseRepository<BlacklistedTokenEntity>
{
    Task DeleteRangeAsync(IEnumerable<BlacklistedTokenEntity> entitiesToRemove, CancellationToken cancellationToken);
}