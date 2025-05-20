using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface ISubcriptionPlanRepository
{
    Task<SubscriptionPlanEntity?> GetByTypeAsync(SubscriptionTypeEnum type, CancellationToken cancellationToken);
}