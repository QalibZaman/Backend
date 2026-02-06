using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Attributes;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Locks.Commands;
using NettoRetail.Application.Enums;
using NettoRetail.Application.Models.Request.Lock;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

/// <summary>
/// Controller for managing entity-level locks.
/// 
/// <para>Supported lockable entities:</para>
/// <list type="bullet">
///   <item><description><c>Role</c></description></item>
/// </list>
/// 
/// <para>All endpoints require specifying the <c>resourceId</c> of the entity in string format. 
/// The string should represent either an integer ID or a UUID, depending on the entity type.</para>
/// 
/// <para>When specifying the entity type in requests, use the string representation of the enum value.
/// For example, to lock a role entity, set <c>entityType = "Role"</c>. The API expects the enum as a string, 
/// not as a numeric value.</para>
/// 
/// <para>Available functionality in this controller:</para>
/// <list type="bullet">
///   <item><description>Acquire a lock on an entity (<c>TryLock</c>)</description></item>
///   <item><description>Release a lock (<c>Unlock</c>)</description></item>
///   <item><description>Refresh the TTL of an existing lock (<c>RefreshLock</c>)</description></item>
///   <item><description>Check if an entity is locked by another user (<c>IsLockedByAnotherUser</c>)</description></item>
/// </list>
/// 
/// <para>This controller ensures that conflicting operations on entities are prevented 
/// by using locks and is intended to be used through the API only.</para>
/// </summary>
[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class LocksController(IMediator mediator)
    : ControllerBase
{
    /// <summary>
    /// Attempts to acquire a lock on a specific entity for the current user.
    /// Only users with the required permissions and a valid shop claim can perform this action.
    /// Intended for use by the front-end when a user wants to perform an edit or modification.
    /// </summary>
    /// <remarks>
    /// User case scenario:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       A user wants to edit a role. They open the role edit modal in the front-end.
    ///       At this moment, this endpoint should be called to acquire a lock on the role.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       If the role is successfully locked for the current user, the API returns <c>204 No Content</c>,
    ///       and the user can safely edit the role.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       If another user already holds the lock on this role, the API returns <c>409 Conflict</c>.
    ///       The front-end can display a message like "This role is currently being edited by another user."
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       If the role does not exist in the database, the API returns <c>400 Bad Request</c>.
    ///       The front-end can display a message like "The selected role does not exist."
    ///     </description>
    ///   </item>
    /// </list>
    /// 
    /// Example request:
    /// <code>
    /// POST /api/v1/locks/lock
    /// {
    ///     "entityType": "Role",
    ///     "resourceId": "123"
    /// }
    /// </code>
    /// <para>
    /// <c>entityType</c> must be specified as the string representation of the enum (e.g., "Role").
    /// <c>resourceId</c> must be provided as a string, representing either an integer ID or a UUID.
    /// </para>
    /// <para>
    /// Supported values for locking:
    /// <list type="bullet">
    ///   <item><description><c>Role</c></description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="lockRequest">The lock request containing the entity type and resource ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="204">Lock was successfully acquired. The user can now edit the entity.</response>
    /// <response code="409">The entity is already locked by another user.</response>
    /// <response code="400">The specified entity does not exist in the database.</response>
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost("lock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TryLock([FromBody]LockRequest lockRequest, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var userId = HttpContext.GetUserId();

        var command = new LockEntityCommand(lockRequest, shopId, userId);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Refreshes the time-to-live (TTL) of an existing lock for the specified entity.
    /// Only the user who owns the lock can refresh it, and a valid shop claim is required.
    /// Intended for use by the front-end to keep a lock active while a user is editing an entity.
    /// </summary>
    /// <remarks>
    /// User case scenario:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       A user is currently editing a role. To prevent the lock from expiring while the user is still making changes,
    ///       the front-end should periodically call this endpoint to refresh the lock TTL.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       If the lock TTL is successfully refreshed, the API returns <c>204 No Content</c>.
    ///       The user can continue editing the entity safely.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       If another user holds the lock or the lock has expired, the API may return <c>409 Conflict</c>.
    ///       The front-end can notify the user that they no longer have exclusive access to edit.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       If the entity does not exist in the database, the API returns <c>400 Bad Request</c>.
    ///       The front-end can display a message like "The selected entity does not exist."
    ///     </description>
    ///   </item>
    /// </list>
    /// 
    /// Example request:
    /// <code>
    /// PUT /api/v1/locks/refresh
    /// {
    ///     "entityType": "Role",
    ///     "resourceId": "123"
    /// }
    /// </code>
    /// <para>
    /// <c>entityType</c> must be specified as the string representation of the enum (e.g., "Role").
    /// <c>resourceId</c> must be provided as a string, representing either an integer ID or a UUID.
    /// </para>
    /// <para>
    /// Supported values for locking:
    /// <list type="bullet">
    ///   <item><description><c>Role</c></description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="lockRequest">The lock request containing the entity type and resource ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="204">Lock TTL was successfully refreshed; the user can continue editing the entity.</response>
    /// <response code="409">The entity lock is held by another user or has expired.</response>
    /// <response code="400">The specified entity does not exist in the database.</response>
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPut("refresh")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Refresh([FromBody]LockRequest lockRequest, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var userId = HttpContext.GetUserId();

        var command = new RefreshLockCommand(lockRequest, shopId, userId);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Releases a lock on the specified entity.
    /// Only the user who owns the lock can unlock it, and a valid shop claim is required.
    /// Intended for use by the front-end when a user finishes editing an entity and wants to release the lock.
    /// </summary>
    /// <remarks>
    /// User case scenario:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       A user has finished editing a role. The front-end should call this endpoint to release the lock
    ///       so that other users can edit the entity.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       If the lock is successfully released, the API returns <c>204 No Content</c>.
    ///       The entity is now available for editing by other users.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       If the entity is locked by another user or the lock cannot be released for some reason,
    ///       the API may return <c>409 Conflict</c>. The front-end can notify the user accordingly.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       If the entity does not exist in the database, the API returns <c>400 Bad Request</c>.
    ///       The front-end can display a message like "The selected entity does not exist."
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Note: If this endpoint is not called, the lock will automatically expire when the TTL (time-to-live) elapses.
    ///     </description>
    ///   </item>
    /// </list>
    /// 
    /// Example request:
    /// <code>
    /// DELETE /api/v1/locks
    /// {
    ///     "entityType": "Role",
    ///     "resourceId": "123"
    /// }
    /// </code>
    /// <para>
    /// <c>entityType</c> must be specified as the string representation of the enum (e.g., "Role").
    /// <c>resourceId</c> must be provided as a string, representing either an integer ID or a UUID.
    /// </para>
    /// <para>
    /// Supported values for locking:
    /// <list type="bullet">
    ///   <item><description><c>Role</c></description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="lockRequest">The lock request containing the entity type and resource ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="204">Lock was successfully released; the entity is now available for other users.</response>
    /// <response code="409">The entity lock is held by another user or cannot be released.</response>
    /// <response code="400">The specified entity does not exist in the database.</response>
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Unlock([FromBody]LockRequest lockRequest, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var userId = HttpContext.GetUserId();
        var command = new UnlockEntityCommand(lockRequest, shopId, userId);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Checks whether the specified entity is currently locked by another user.
    /// Only users with the required permissions and a valid shop claim can perform this check.
    /// Intended for use by the front-end to determine if an entity is currently blocked (e.g., before deletion).
    /// </summary>
    /// <remarks>
    /// User case scenario:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       A user wants to delete a role. They click the "Delete" button in the front-end.
    ///       Before proceeding, the front-end calls this endpoint to check if the role is locked by another user.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       If the entity is currently locked by another user, the API returns <c>true</c> with <c>200 OK</c>.
    ///       The front-end should prevent deletion and display a message like "This role is currently being edited by another user."
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       If the entity is not locked by another user, the API returns <c>false</c> with <c>200 OK</c>.
    ///       The front-end can safely allow the deletion.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       No other lock-related methods (TryLock, Refresh, Unlock) are required in this scenario,
    ///       because the user is only checking availability for deletion, not editing.
    ///     </description>
    ///   </item>
    /// </list>
    /// 
    /// Example request:
    /// <code>
    /// POST /api/v1/locks/check
    /// {
    ///     "entityType": "Role",
    ///     "resourceId": "123"
    /// }
    /// </code>
    /// <para>
    /// <c>entityType</c> must be specified as the string representation of the enum (e.g., "Role").
    /// <c>resourceId</c> must be provided as a string, representing either an integer ID or a UUID.
    /// </para>
    /// <para>
    /// Supported values for locking:
    /// <list type="bullet">
    ///   <item><description><c>Role</c></description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="lockRequest">The lock request containing the entity type and resource ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">
    /// Returns <c>true</c> if the entity is currently locked by another user; otherwise <c>false</c>.
    /// </response>
    /// <response code="400">The specified entity does not exist in the database.</response>
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost("check")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IsEntityLockedByAnotherUser([FromBody] LockRequest lockRequest, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var userId = HttpContext.GetUserId();

        var command = new IsEntityLockedByAnotherUserCommand(lockRequest, shopId, userId);
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}
