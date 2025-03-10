using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.Services;

public class FailedAuthAttemptService(IFailedAuthAttemptRepository failedAuthAttemptRepository, IOptions<UserBlockOptions> userBlockOptions) : IFailedAuthAttemptService
{
    private readonly UserBlockOptions _userBlockOptions = userBlockOptions.Value;
    private IFailedAuthAttemptRepository _loginAttemptRepository = failedAuthAttemptRepository;

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
        if (attempt.FailedAttempts >= _userBlockOptions.FailedAttemptThreshold)
        {
            attempt.LockoutEndTime = DateTime.UtcNow.AddMinutes(_userBlockOptions.LockoutDurationInMinutes);
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

    public bool HasBlockDurationElapsed(FailedAuthAttemptEntity attempt)
    {
        return attempt != null &&
               attempt.LastFailedTime.AddHours(_userBlockOptions.BlockDurationInHours) < DateTime.UtcNow;
    }
}