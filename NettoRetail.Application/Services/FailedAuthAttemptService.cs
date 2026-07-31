using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.Services;

public class FailedAuthAttemptService(IFailedAuthAttemptRepository failedAuthAttemptRepository, IOptions<UserBlockOptions> userBlockOptions) : IFailedAuthAttemptService
{
    private readonly UserBlockOptions _userBlockOptions = userBlockOptions.Value;

    public bool IsAccountLocked(FailedAuthAttemptEntity? attempt)
    {
        return attempt is { LockoutEndTime: not null } &&
               attempt.LockoutEndTime > DateTime.UtcNow;
    }

    public async Task<FailedAuthAttemptEntity> IncrementFailedAttemptsAsync(FailedAuthAttemptEntity? attempt, Guid staffId, CancellationToken cancellationToken)
    {
        if (attempt == null)
        {
            attempt = new FailedAuthAttemptEntity { StaffId = staffId };
            await failedAuthAttemptRepository.AddAsync(attempt, cancellationToken);
        }

        attempt.FailedAttempts++;
        attempt.LastFailedTime = DateTime.UtcNow;
        if (attempt.FailedAttempts >= _userBlockOptions.FailedAttemptThreshold)
        {
            attempt.LockoutEndTime =
                DateTime.UtcNow.AddMinutes(_userBlockOptions.LockoutDurationInMinutes);
        }

        await failedAuthAttemptRepository.UpdateAsync(attempt, cancellationToken);
        return attempt;
    }

    public async Task ResetFailedAttemptsAsync(FailedAuthAttemptEntity attempt, CancellationToken cancellationToken)
    {
        attempt.FailedAttempts = 0;
        attempt.LockoutEndTime = null;
        await failedAuthAttemptRepository.UpdateAsync(attempt, cancellationToken);
    }

    public bool HasBlockDurationElapsed(FailedAuthAttemptEntity? attempt)
    {
        return attempt != null &&
               attempt.LastFailedTime.AddHours(_userBlockOptions.BlockDurationInHours) < DateTime.UtcNow;
    }

    public int AvailableAttempts(FailedAuthAttemptEntity attempt)
    {
        return _userBlockOptions.FailedAttemptThreshold - attempt.FailedAttempts;
    }
}