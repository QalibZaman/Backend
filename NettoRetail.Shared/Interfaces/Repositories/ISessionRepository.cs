using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface ISessionRepository : IBaseRepository<SessionEntity>
{
    Task<SessionEntity?> UpdateAsync(Specification<SessionEntity> specification, string? currentJwt, string? currentRefreshToken, DateTime lastActivityTime, DateTime expiresAt, CancellationToken cancellationToken);
    Task<SessionEntity?> GetByJwtAsync(string jwt);
    Task TerminateSessionsAsync(Specification<SessionEntity> specification, CancellationToken cancellationToken);
}
