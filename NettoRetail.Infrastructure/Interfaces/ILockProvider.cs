namespace NettoRetail.Infrastructure.Interfaces;

/// <summary>
/// Defines a provider for entity-level locks with support for a time-to-live (TTL).
/// 
/// This interface abstracts the locking mechanism and allows different implementations
/// to be used. Currently, <c>HardLockWithTtlProviderService</c> is the only implementation,
/// but in the future other mechanisms such as distributed locks, optimistic locks, or
/// soft locks could be supported.
/// 
/// The interface is intended to be used via <see cref="EntityLockService"/>, rather than
/// directly, to manage concurrency and prevent conflicting operations on entities.
/// </summary>
public interface ILockProvider
{
    Task<bool> TryLockAsync(
        string resourceType,
        string resourceId,
        int shopId,
        Guid userId,
        TimeSpan ttl,
        CancellationToken cancellationToken);

    Task<bool> RefreshLockAsync(
        string resourceType,
        string resourceId,
        int shopId,
        Guid userId,
        TimeSpan ttl,
        CancellationToken cancellationToken);

    Task<bool> UnlockAsync(
        string resourceType,
        string resourceId,
        int shopId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<bool> IsLockedByAnotherUserAsync(
        string resourceType,
        string resourceId,
        int shopId,
        Guid currentUserId,
        TimeSpan ttl,
        CancellationToken cancellationToken);
}