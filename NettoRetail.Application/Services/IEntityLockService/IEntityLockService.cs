using NettoRetail.Application.Enums;

namespace NettoRetail.Application.Services.IEntityLockService;

public interface IEntityLockService
{
    /// <summary>
    /// Attempts to acquire a lock for the specified resource.
    /// Throws an exception if the entity does not exist or if the lock
    /// is already held by another user.
    /// This method is intended for internal use only by
    /// <c>LockEntityCommandHandler</c>.
    /// </summary>
    /// <param name="entityType">The lockable entity type.</param>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="shopId">The shop identifier.</param>
    /// <param name="userId">The identifier of the user attempting to acquire the lock.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task TryLockAsync(
        LockableEntitiesEnum entityType,
        string resourceId,
        int shopId,
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Refresh the lock for the specified resource if it is currently held
    /// by the provided user.
    /// This method is intended for internal use only by
    /// <c>RefreshLockCommandHandler</c>.
    /// </summary>
    /// <param name="entityType">The lockable entity type.</param>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="shopId">The shop identifier.</param>
    /// <param name="userId">The identifier of the user who owns the lock.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous unlock operation.</returns>
    Task RefreshLockAsync(
        LockableEntitiesEnum entityType,
        string resourceId,
        int shopId,
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Releases the lock for the specified resource if it is currently held
    /// by the provided user.
    /// This method is intended for internal use only by
    /// <c>UnlockEntityCommandHandler</c>.
    /// </summary>
    /// <param name="entityType">The lockable entity type.</param>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="shopId">The shop identifier.</param>
    /// <param name="userId">The identifier of the user who owns the lock.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous unlock operation.</returns>
    Task UnlockAsync(
        LockableEntitiesEnum entityType,
        string resourceId,
        int shopId,
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether there is an active lock on the specified resource
    /// created by another user.
    /// This method is intended for internal use only by
    /// <c>IsEntityBlockedByAnotherUserCommandHandler</c>.
    /// </summary>
    /// <param name="entityType">The lockable entity type.</param>
    /// <param name="resourceId">The resource identifier.</param>
    /// <param name="shopId">The shop identifier.</param>
    /// <param name="currentUserId">The current user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// <c>true</c> if the resource is locked by another user; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> IsLockedByAnotherUserAsync(
        LockableEntitiesEnum entityType,
        string resourceId,
        int shopId,
        Guid currentUserId,
        CancellationToken cancellationToken);
}