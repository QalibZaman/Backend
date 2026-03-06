using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories;

/// <summary>
/// Repository for generic locks on entities, supporting TTL and per-shop locks.
/// Intended for internal use only within the EntityLockService.
/// </summary>
public interface IEntityLockRepository : IBaseRepository<EntityLock>
{
    /// <summary>
    /// Attempts to acquire a lock on a specific resource for the given user.
    /// If the resource is already locked by the same user, the lock is updated (refreshed).
    /// If the resource is locked by another user but the lock's TTL has expired, the lock is overwritten
    /// and assigned to the current user.
    /// If the resource is locked by another user and the TTL has not expired, the method returns false.
    /// </summary>
    Task<bool> TryLockAsync(
        string resourceType,
        string resourceId,
        int shopId,
        Guid userId,
        TimeSpan ttl,
        CancellationToken cancellationToken);

    /// <summary>
    /// Refreshes a lock held by the current user if TTL is still active.
    /// </summary>
    Task<bool> RefreshLockAsync(
        string resourceType,
        string resourceId,
        int shopId,
        Guid userId,
        TimeSpan ttl,
        CancellationToken cancellationToken);

    /// <summary>
    /// Unlocks a resource for the given user, TTL is ignored.
    /// </summary>
    Task<bool> UnlockAsync(
        string resourceType,
        string resourceId,
        int shopId,
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Checks if a resource is currently locked by another user with unexpired TTL.
    /// </summary>
    Task<bool> IsLockedByAnotherUserAsync(
        string resourceType,
        string resourceId,
        int shopId,
        Guid currentUserId,
        TimeSpan ttl,
        CancellationToken cancellationToken);
}
