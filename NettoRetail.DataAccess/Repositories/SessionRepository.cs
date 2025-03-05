using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class SessionRepository(AppDbContext dbContext) : BaseRepository<SessionEntity>(dbContext), ISessionRepository
{
    public async Task UpdateAsync(Specification<SessionEntity> specification, string currentJwt, DateTime lastActivityTime, CancellationToken cancellationToken)
    {
        var entitityToUpdate = await _dbSet.Where(specification.Criteria).FirstAsync(cancellationToken);
        entitityToUpdate.LastActivityTime = lastActivityTime;
        entitityToUpdate.Jwt = currentJwt;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
