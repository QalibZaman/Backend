using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface ILoginAttemptRepository : IBaseRepository<LoginAttemptEntity>
{
    Task IncrementFailedAttemptsAsync(LoginAttemptEntity attempt, Guid staffId, CancellationToken cancellationToken);
    Task ResetFailedAttemptsAsync(LoginAttemptEntity attempt, CancellationToken cancellationToken);
}
