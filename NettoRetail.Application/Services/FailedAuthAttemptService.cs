using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services;

public class FailedAuthAttemptService(IFailedAuthAttemptRepository failedAuthAttemptRepository) : IFailedAuthAttemptService
{
    private readonly IFailedAuthAttemptRepository _loginAttemptRepository = failedAuthAttemptRepository;

    public bool IsAccountLocked(FailedAuthAttemptEntity attempt)
    {
        return attempt != null &&
            attempt.LockoutEndTime.HasValue &&
            attempt.LockoutEndTime > DateTime.UtcNow;
    }

    public async Task IncrementFailedAttemptsAsync(FailedAuthAttemptEntity attempt, Guid staffId, CancellationToken cancellationToken)
    {
        if (attempt == null)
        {
            attempt = new FailedAuthAttemptEntity { StaffId = staffId };
            await _loginAttemptRepository.AddAsync(attempt, cancellationToken);
        }

        attempt.FailedAttempts++;
        attempt.LastFailedTime = DateTime.UtcNow;
        if (attempt.FailedAttempts >= 3)
        {
            attempt.LockoutEndTime = DateTime.UtcNow.AddMinutes(30);
        }

        await _loginAttemptRepository.UpdateAsync(attempt, cancellationToken);
    }

    public async Task ResetFailedAttemptsAsync(FailedAuthAttemptEntity attempt, CancellationToken cancellationToken)
    {
        if (attempt != null)
        {
            attempt.FailedAttempts = 0;
            attempt.LockoutEndTime = null;
            await _loginAttemptRepository.UpdateAsync(attempt, cancellationToken);
        }
    }
}