using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class EntityLockRepository : BaseRepository<EntityLock>, IEntityLockRepository
{
    private new readonly AppDbContext _dbContext;

    public EntityLockRepository(AppDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> TryLockAsync(
    string resourceType,
    string resourceId,
    int shopId,
    Guid userId,
    TimeSpan ttl,
    CancellationToken cancellationToken)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var ttlBorder = nowUtc - ttl;

        // Atiomic INSERT or UPDATE if TTL expired or it is the same user.
        // If there is already a lock on the same key from another user with an unexpired TTL, returns false.
        var rows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $@"
            INSERT INTO ""EntityLocks"" (""ResourceType"", ""ResourceId"", ""ShopId"", ""LockedByUserId"", ""LockedAtUtc"")
            VALUES ({resourceType}, {resourceId}, {shopId}, {userId}, {nowUtc})
            ON CONFLICT (""ResourceType"", ""ResourceId"", ""ShopId"")
            DO UPDATE
            SET ""LockedByUserId"" = {userId},
                ""LockedAtUtc"" = {nowUtc}
            WHERE ""EntityLocks"".""LockedByUserId"" = {userId}
               OR ""EntityLocks"".""LockedAtUtc"" < {ttlBorder};
            ",
            cancellationToken);

        return rows == 1;
    }

    public async Task<bool> RefreshLockAsync(
    string resourceType,
    string resourceId,
    int shopId,
    Guid userId,
    TimeSpan ttl,
    CancellationToken cancellationToken)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var ttlBorder = nowUtc - ttl;

        // Update of lock only if:
        // The lock is held by the current user and lock is still active (TTL has not expired)
        var rows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $@"
            UPDATE ""EntityLocks""
            SET ""LockedAtUtc"" = {nowUtc}
            WHERE ""ResourceType"" = {resourceType}
              AND ""ResourceId"" = {resourceId}
              AND ""ShopId"" = {shopId}
              AND ""LockedByUserId"" = {userId}
              AND ""LockedAtUtc"" >= {ttlBorder};
            ",
            cancellationToken);

        return rows == 1;
    }

    public async Task<bool> UnlockAsync(
    string resourceType,
    string resourceId,
    int shopId,
    Guid userId,
    CancellationToken cancellationToken)
    {
        // DELETE the lock if it belongs to this user, TTL is not checked.
        var rows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $@"
            DELETE FROM ""EntityLocks""
            WHERE ""ResourceType"" = {resourceType}
              AND ""ResourceId"" = {resourceId}
              AND ""ShopId"" = {shopId}
              AND ""LockedByUserId"" = {userId};
            ",
            cancellationToken);

        return rows == 1;
    }

    public async Task<bool> IsLockedByAnotherUserAsync(
    string resourceType,
    string resourceId,
    int shopId,
    Guid currentUserId,
    TimeSpan ttl,
    CancellationToken cancellationToken)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var ttlBorder = nowUtc - ttl;

        // Checking for an active lock from another user.
        return await _dbContext.EntityLocks.AnyAsync(
            l => l.ResourceType == resourceType
                 && l.ResourceId == resourceId
                 && l.ShopId == shopId
                 && l.LockedByUserId != currentUserId
                 && l.LockedAtUtc >= ttlBorder,
            cancellationToken);
    }
}
