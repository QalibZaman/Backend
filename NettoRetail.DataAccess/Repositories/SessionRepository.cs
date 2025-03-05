using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class SessionRepository : BaseRepository<SessionEntity>, ISessionRepository
{
    public SessionRepository(AppDbContext dbContext)
        : base(dbContext) { }

    public async Task UpdateAsync(Specification<SessionEntity> specification, string currentJwt, DateTime lastActivityTime, CancellationToken cancellationToken)
    {
        var entitityToUpdate = await _dbSet.Where(specification.Criteria).FirstAsync(cancellationToken);
        entitityToUpdate.LastActivityTime = lastActivityTime;
        entitityToUpdate.Jwt = currentJwt;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
