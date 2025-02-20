using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using NettoHealth.DataAccess.Context;
using NettoHealth.Domain.Entities;
using NettoHealth.Shared.Interfaces.Repositories;
using NettoHealth.Shared.Specifications;
using NettoHealth.DataAccess.Extensions;

namespace NettoHealth.DataAccess.Repositories;

public class HealthcareProviderRepository(AppDbContext dbContext, IMapper mapper)
    : BaseRepository<HealthcareProviderEntity>(dbContext, mapper),
    IHealthcareProviderRepository
{
    public async Task<HealthcareProviderEntity?> GetSingleBySpecAsync(Specification<HealthcareProviderEntity> specification, CancellationToken cancellationToken)
    {
        var query = dbContext.Set<HealthcareProviderEntity>().AsQueryable()
                             .ApplySpecification(specification);

        return await query.FirstOrDefaultAsync(specification.Criteria, cancellationToken);
    }
}