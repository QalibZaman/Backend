using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Interfaces;

public interface IFailedAuthAttemptService
{
    bool IsAccountLocked(FailedAuthAttemptEntity attempt);
    Task<FailedAuthAttemptEntity> IncrementFailedAttemptsAsync(FailedAuthAttemptEntity? attempt, Guid accountId, CancellationToken cancellationToken, int? lockoutDurationInMinutes = null);
    Task ResetFailedAttemptsAsync(FailedAuthAttemptEntity attempt, CancellationToken cancellationToken);
    bool HasBlockDurationElapsed(FailedAuthAttemptEntity? attempt);
    int AvailableAttempts(FailedAuthAttemptEntity attempt);
}