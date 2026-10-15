using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface ISubscriptionRepository : IBaseRepository<SubscriptionEntity>
{
    Task<SubscriptionEntity?> GetActiveByShopIdAsync(int shopId,
        CancellationToken cancellationToken);
}