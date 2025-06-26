using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Seeders.Interfaces;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Seeders
{
    public class FeatureSubscriptionPlanSeeder(AppDbContext dbContext) : IDatabaseSeeder
    {
        public async Task SeedAsync(CancellationToken cancellationToken)
        {
            var hasAnyLink = await dbContext.Set<FeatureEntity>()
                .Include(f => f.SubscriptionPlans)
                .AnyAsync(f => f.SubscriptionPlans.Any(), cancellationToken);

            if (hasAnyLink)
                return;

            var plans = await dbContext.SubscriptionPlans.ToListAsync(cancellationToken);
            var features = await dbContext.Features.ToListAsync(cancellationToken);

            foreach (var plan in plans)
            {
                if (features.Any())
                {
                    plan.Features ??= new List<FeatureEntity>();
                    if (!plan.Features.Any())
                        plan.Features.Add(features.First());
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
