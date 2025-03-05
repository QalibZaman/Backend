using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface ISessionRepository : IBaseRepository<SessionEntity>
{
    Task UpdateAsync(Specification<SessionEntity> specification, string currentJwt, DateTime lastActivityTime, CancellationToken cancellationToken);
}