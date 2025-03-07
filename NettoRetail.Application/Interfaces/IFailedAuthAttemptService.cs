using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Interfaces;

public interface IFailedAuthAttemptService
{
    bool IsAccountLocked(FailedAuthAttemptEntity attempt);
    Task IncrementFailedAttemptsAsync(FailedAuthAttemptEntity attempt, Guid staffId, CancellationToken cancellationToken);
    Task ResetFailedAttemptsAsync(FailedAuthAttemptEntity attempt, CancellationToken cancellationToken);
}