using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Interfaces;

public interface ILoginAttemptService
{
    bool IsAccountLocked(LoginAttemptEntity attempt);
    Task IncrementFailedAttemptsAsync(LoginAttemptEntity attempt, Guid staffId, CancellationToken cancellationToken);
    Task ResetFailedAttemptsAsync(LoginAttemptEntity attempt, CancellationToken cancellationToken);
}