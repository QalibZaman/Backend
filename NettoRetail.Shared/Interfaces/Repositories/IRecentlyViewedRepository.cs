using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories;
public interface IRecentlyViewedRepository : IBaseRepository<RecentlyViewedProductEntity>
{
    Task MergeAsync(Guid accountId, IEnumerable<RecentlyViewedProductEntity> guestItems, CancellationToken cancellationToken);
}
