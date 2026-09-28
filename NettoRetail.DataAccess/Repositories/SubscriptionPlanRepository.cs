using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories
{
    public class SubscriptionPlanRepository(AppDbContext dbContext) : BaseRepository<SubscriptionPlanEntity>(dbContext), ISubscriptionPlanRepository;
}
