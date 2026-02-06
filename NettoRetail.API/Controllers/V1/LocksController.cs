using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Attributes;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Locks.Commands;
using NettoRetail.Application.CQRS.Locks.Queries;
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
    /// </summary>
    /// <remarks>
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
    /// </remarks>
    /// <param name="lockRequest">The lock request containing the entity type and resource ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="204">Lock was successfully acquired.</response>
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
    /// </summary>
    /// <remarks>
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
    /// </remarks>
    /// <param name="lockRequest">The lock request containing the entity type and resource ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="204">Lock TTL was successfully refreshed.</response>
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
    /// </summary>
    /// <remarks>
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
    /// </remarks>
    /// <param name="lockRequest">The lock request containing the entity type and resource ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="204">Lock was successfully released.</response>
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
    /// </summary>
    /// <remarks>
    /// Example request:
    /// <code>
    /// GET /api/v1/locks?entityType=Role&resourceId=123
    /// </code>
    /// <para>
    /// <c>entityType</c> must be specified as the string representation of the enum (e.g., "Role").
    /// <c>resourceId</c> must be provided as a string, representing either an integer ID or a UUID.
    /// </para>
    /// </remarks>
    /// <param name="lockRequest">The lock request containing the entity type and resource ID as query parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">Returns <c>true</c> if the entity is locked by another user; otherwise <c>false</c>.</response>
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IsEntityLockedByAntotherUser([FromQuery]LockRequest lockRequest, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var userId = HttpContext.GetUserId();

        var command = new IsEntityLockedByAnotherUserCommand(lockRequest, shopId, userId);
        var result = await mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<bool>(result));
    }
}
