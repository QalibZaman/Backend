using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace NettoRetail.DataAccess.Repositories
{
    public class PriceHistoryRepository : BaseRepository<PriceHistoryEntity>, IPriceHistoryRepository
    {
        public PriceHistoryRepository(AppDbContext context) : base(context) { }

        public async Task<PriceHistoryEntity?> GetActivePriceAsync(int planId, DateTime? date = null, CancellationToken cancellationToken = default)
        {
            var now = date ?? DateTime.UtcNow;
            return await _dbSet
                .Where(ph => ph.SubscriptionPlanId == planId
                             && ph.StartDateTime <= now
                             && (ph.EndDateTime == null || ph.EndDateTime > now))
                .OrderByDescending(ph => ph.StartDateTime)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}