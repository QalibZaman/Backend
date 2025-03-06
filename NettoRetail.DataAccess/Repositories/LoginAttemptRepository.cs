using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class LoginAttemptRepository(AppDbContext dbContext)
    : BaseRepository<LoginAttemptEntity>(dbContext), ILoginAttemptRepository
{
    public async Task IncrementFailedAttemptsAsync(LoginAttemptEntity attempt, Guid staffId, CancellationToken cancellationToken)
    {
        if (attempt == null)
        {
            attempt = new LoginAttemptEntity { StaffId = staffId };
            await _dbContext.LoginAttempts.AddAsync(attempt, cancellationToken);
        }

        attempt.FailedAttempts++;
        attempt.LastFailedLoginTime = DateTime.UtcNow;
        if (attempt.FailedAttempts >= 3)
        {
            attempt.LockoutEndTime = DateTime.UtcNow.AddMinutes(30);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetFailedAttemptsAsync(LoginAttemptEntity attempt, CancellationToken cancellationToken)
    {
        if (attempt != null)
        {
            attempt.FailedAttempts = 0;
            attempt.LockoutEndTime = null;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
