using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Interfaces;

public interface IActivationEmailAttemptService
{
    Task IncrementSentEmailsAsync(ActivationEmailAttemptEntity activationEmailAttemptEntity, Guid staffId, CancellationToken cancellationToken);
    bool HasBlockDurationElapsed(ActivationEmailAttemptEntity? activationEmailEntity);
    bool HasAttemptsRemaining(ActivationEmailAttemptEntity? attempt);
    Task ResetAttemptsAsync(ActivationEmailAttemptEntity attempt, CancellationToken cancellationToken);
}