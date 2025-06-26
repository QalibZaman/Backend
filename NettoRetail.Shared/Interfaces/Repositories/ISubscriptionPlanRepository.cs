using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface ISubscriptionPlanRepository : IBaseRepository<SubscriptionPlanEntity>
{
    Task<SubscriptionPlanEntity?> GetByTypeAsync(SubscriptionTypeEnum type, CancellationToken cancellationToken);
    Task<List<SubscriptionPlanEntity>> ListAllAsync(CancellationToken cancellationToken = default);

}