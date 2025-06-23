using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Seeders.Interfaces;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Options;

namespace NettoRetail.DataAccess.Seeders;

public class SubscriptionPlanSeeder(AppDbContext dbContext, IOptions<SubscriptionOptions> options) : IDatabaseSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (dbContext.SubscriptionPlans.Any())
            return;

        var config = options.Value;

        var plans = new List<SubscriptionPlanEntity>
        {
            new()
            {
                Id = 1,
                Type = SubscriptionTypeEnum.Free,
                Name = "Free",
                Description = "Free plan",
                MonthlyPrice = 0,
                Moto = "Try the app for free",
                SubscriptionId = config.FreePlanId
            },
            new()
            {
                Id = 2,
                Type = SubscriptionTypeEnum.Super,
                Name = "Super",
                Description = "Super plan",
                MonthlyPrice = 20,
                Moto = "Super features",
                SubscriptionId = config.SuperPlanId
            },
            new()
            {
                Id = 3,
                Type = SubscriptionTypeEnum.Business,
                Name = "Business",
                Description = "Business plan",
                MonthlyPrice = 30,
                Moto = "Full business power",
                SubscriptionId = config.BussinessPlanId
            }
        };

        dbContext.SubscriptionPlans.AddRange(plans);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
