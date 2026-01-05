using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class InvitationTokenRepository(AppDbContext dbContext) : BaseRepository<InvitationTokenEntity>(dbContext), IInvitationTokenRepository
{
    public async Task<InvitationTokenEntity?> GetByHashAsync(
        Specification<InvitationTokenEntity> specification,
        CancellationToken cancellationToken)
    {
        var query = _dbSet.AsQueryable();

        if (specification.Criteria != null)
            query = query.Where(specification.Criteria);

        foreach (var include in specification.Includes)
            query = query.Include(include);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task InvalidateAllActiveAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        await _dbSet
             .Where(t => t.AccountId == accountId && t.UsedAt == null && t.InvalidatedAt == null)
             .ExecuteUpdateAsync(s => s.SetProperty(x => x.InvalidatedAt, now), cancellationToken);
    }
}