using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class SubscriptionRepository(AppDbContext dbContext)
    : BaseRepository<SubscriptionEntity>(dbContext), ISubscriptionRepository
{
    public async Task<SubscriptionEntity?> GetActiveByShopIdAsync(
        int shopId,
        CancellationToken cancellationToken)
    {
        return await _dbSet
            .SingleOrDefaultAsync(
                subscription => subscription.ShopId == shopId &&
                                subscription.Status == SubscriptionStatusEnum.Active,
                cancellationToken);
    }
}
