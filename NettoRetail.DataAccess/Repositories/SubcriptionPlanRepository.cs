using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class SubcriptionPlanRepository(AppDbContext context)
    : BaseRepository<SubscriptionPlanEntity>(context), ISubcriptionPlanRepository
{
    public async Task<SubscriptionPlanEntity?> GetByTypeAsync(SubscriptionTypeEnum type, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Type == type, cancellationToken);
    }

    public async Task<List<SubscriptionPlanEntity>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }
}
