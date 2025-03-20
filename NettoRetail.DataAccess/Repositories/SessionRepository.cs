using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class SessionRepository(AppDbContext dbContext) : BaseRepository<SessionEntity>(dbContext), ISessionRepository
{
    public async Task UpdateAsync(
        Specification<SessionEntity> specification,
        string? currentJwt,
        string? currentRefreshToken,
        DateTime lastActivityTime,
        DateTime expiresAt,
        CancellationToken cancellationToken)
    {
        var entityToUpdate = await _dbSet.Where(specification.Criteria).FirstOrDefaultAsync(cancellationToken);
        if (entityToUpdate != null)
        {
            entityToUpdate.LastActivityTime = lastActivityTime;
            entityToUpdate.ExpiresAt = expiresAt;

            if (currentJwt != null)
                entityToUpdate.Jwt = currentJwt;

            if (currentRefreshToken != null)
                entityToUpdate.RefreshToken = currentRefreshToken;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
