using NettoHealth.Domain.Entities;
using NettoHealth.Shared.Specifications;

namespace NettoHealth.Shared.Interfaces.Repositories;

public interface IHealthcareProviderRepository : IBaseRepository<HealthcareProviderEntity>
{
    Task<HealthcareProviderEntity?> GetSingleBySpecAsync(Specification<HealthcareProviderEntity> specification, CancellationToken cancellationToken);
}