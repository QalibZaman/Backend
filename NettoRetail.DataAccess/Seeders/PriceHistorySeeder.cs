using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Seeders.Interfaces;

namespace NettoRetail.DataAccess.Seeders
{
    public class PriceHistorySeeder(AppDbContext dbContext) : IDatabaseSeeder
    {
        public async Task SeedAsync(CancellationToken cancellationToken)
        {
            if (await dbContext.PriceHistories.AnyAsync(cancellationToken))
                return;

            var subscriptionPlans = await dbContext.SubscriptionPlans.ToListAsync(cancellationToken);

            if (!subscriptionPlans.Any())
                return;

            var priceHistories = subscriptionPlans
                .Select(plan => new PriceHistoryEntity
                {
                    SubscriptionPlanId = plan.Id,
                    Price = plan.MonthlyPrice,
                    StartDateTime = DateTime.UtcNow,
                    EndDateTime = null
                })
                .ToList();

            if (priceHistories.Count == 0)
                return;

            dbContext.PriceHistories.AddRange(priceHistories);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
