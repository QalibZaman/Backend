using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services;

public class LoginAttemptService(ILoginAttemptRepository loginAttemptRepository) : ILoginAttemptService
{
    private readonly ILoginAttemptRepository _loginAttemptRepository = loginAttemptRepository;

    public bool IsAccountLocked(LoginAttemptEntity attempt)
    {
        return attempt != null &&
            attempt.LockoutEndTime.HasValue &&
            attempt.LockoutEndTime > DateTime.UtcNow;
    }

    public async Task IncrementFailedAttemptsAsync(LoginAttemptEntity attempt, Guid staffId, CancellationToken cancellationToken)
    {
        if (attempt == null)
        {
            attempt = new LoginAttemptEntity { StaffId = staffId };
            await _loginAttemptRepository.AddAsync(attempt, cancellationToken);
        }

        attempt.FailedAttempts++;
        attempt.LastFailedLoginTime = DateTime.UtcNow;
        if (attempt.FailedAttempts >= 3)
        {
            attempt.LockoutEndTime = DateTime.UtcNow.AddMinutes(30);
        }

        await _loginAttemptRepository.UpdateAsync(attempt, cancellationToken);
    }

    public async Task ResetFailedAttemptsAsync(LoginAttemptEntity attempt, CancellationToken cancellationToken)
    {
        if (attempt != null)
        {
            attempt.FailedAttempts = 0;
            attempt.LockoutEndTime = null;
            await _loginAttemptRepository.UpdateAsync(attempt, cancellationToken);
        }
    }
}