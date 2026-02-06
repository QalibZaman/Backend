using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Infrastructure.Services;

/// <summary>
/// Provides hard locking functionality with a TTL (time-to-live) for resources.
/// Delegates the actual lock operations to an <see cref="IEntityLockRepository"/>.
/// 
/// Supports the full lifecycle of a lock:
/// <list type="bullet">
///   <item><description>TryLockAsync — attempts to acquire a lock for a resource.</description></item>
///   <item><description>RefreshLockAsync — refreshes an existing lock's TTL.</description></item>
///   <item><description>UnlockAsync — releases a lock held by a user.</description></item>
///   <item><description>IsLockedByAnotherUserAsync — checks if a resource is locked by someone else.</description></item>
/// </list>
/// This service is an infrastructure-level implementation of <see cref="ILockProvider"/>.
/// It is intended to be used via <see cref="EntityLockService"/> rather than directly.
/// </summary>
public class HardLockWithTtlProviderService : ILockProvider
{
    private readonly IEntityLockRepository _lockRepository;

    public HardLockWithTtlProviderService(IEntityLockRepository lockRepository)
    {
        _lockRepository = lockRepository;
    }

    public Task<bool> TryLockAsync(
        string resourceType,
        string resourceId,
        int shopId,
        Guid userId,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        return _lockRepository.TryLockAsync(
            resourceType,
            resourceId,
            shopId,
            userId,
            ttl,
            cancellationToken);
    }

    public Task<bool> RefreshLockAsync(
        string resourceType,
        string resourceId,
        int shopId,
        Guid userId,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        return _lockRepository.RefreshLockAsync(
            resourceType,
            resourceId,
            shopId,
            userId,
            ttl,
            cancellationToken);
    }

    public Task<bool> UnlockAsync(
        string resourceType,
        string resourceId,
        int shopId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return _lockRepository.UnlockAsync(
            resourceType,
            resourceId,
            shopId,
            userId,
            cancellationToken);
    }

    public Task<bool> IsLockedByAnotherUserAsync(
        string resourceType,
        string resourceId,
        int shopId,
        Guid currentUserId,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        return _lockRepository.IsLockedByAnotherUserAsync(
            resourceType,
            resourceId,
            shopId,
            currentUserId,
            ttl,
            cancellationToken);
    }
}
