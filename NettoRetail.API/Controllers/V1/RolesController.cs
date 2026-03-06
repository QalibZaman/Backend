using Asp.Versioning;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Attributes;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Application.CQRS.Roles.Queries;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Roles;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
[ApiController]
public class RolesController(IMediator mediator) : ControllerBase
{
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateRoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateRoleResponse>>> AddRole([FromBody] CreateRoleRequest model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var command = new CreateRoleCommand(model, shopId);
        var result = await mediator.Send(command, cancellationToken);
        var response = new ApiResponse<CreateRoleResponse>(result);
        return Ok(response);
    }

    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRole(int id, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var command = new DeleteRoleCommand(id, shopId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<RoleResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<RoleResponse>>>> GetRoles([FromQuery] PaginationRequest model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var query = new GetRolesQuery(model, shopId);
        var result = await mediator.Send(query, cancellationToken);
        var response = new ApiResponse<PaginatedResponse<RoleResponse>>(result);
        return Ok(response);
    }

    /// <summary>
    /// Updates an existing role in the system for the current shop.
    /// </summary>
    /// <param name="roleId">Identifier of the role to be updated. Must be a positive integer representing an existing role.</param>
    /// <param name="model">
    /// Request model containing role update data.
    ///
    /// <para><b>Name</b>:</para>
    /// Role name is required, must be between 2 and 50 characters long.
    /// Allowed characters are Latin letters (A-Z, a-z), digits (0-9), hyphen (-) and underscore (_).
    /// Whitespaces are not allowed.
    ///
    /// <para><b>Description</b>:</para>
    /// Optional field. Maximum length is 500 characters.
    /// Allowed characters are Latin letters, digits, spaces, and common punctuation.
    /// Leading and trailing spaces are not allowed.
    ///
    /// <para><b>PermissionIds</b>:</para>
    /// Must contain a list of permission identifiers.
    /// The list must not be empty and must contain unique values only.
    ///
    /// Allowed permission identifiers are:
    /// 1 — Inventory management
    /// 2 — Customers management
    /// 3 — Orders management
    /// 4 — Admins management
    /// 5 — System settings
    ///
    /// Any other permission identifier values are considered invalid.
    /// </param>
    /// <param name="cancellationToken">Token used to observe cancellation requests.</param>
    /// <returns>
    /// Returns <see cref="IActionResult"/> representing the result of the operation.
    /// The method returns NoContent (204) if the role is successfully updated.
    /// </returns>
    /// <remarks>
    /// The endpoint requires authorization based on system permissions and shop claim policy.
    /// </remarks>
    [RequiresAnyPermission(Permissions.SYSTEM_MANAGEMENT, Permissions.ADMINS_MANAGEMENT)]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPut("{roleId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EditRole(int roleId, [FromBody] UpdateRoleRequest model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var roleUpdateCommand = new RoleUpdateCommand(roleId, shopId, model);
        await mediator.Send(roleUpdateCommand, cancellationToken);
        return NoContent();
    }

    // This endpoint exists to give all needed information for US-3.4 Edit role.
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("get-roles-with-permissions")]
    [ProducesResponseType(typeof(ApiResponse<List<GetRolePermissionsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateRoleResponse>>> GetRolesWithPermissions(CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var command = new GetRolePermissionQuery(shopId);
        var result = await mediator.Send(command, cancellationToken);
        var response = new ApiResponse<List<GetRolePermissionsResponse>>(result);

        return Ok(response);
    }
}
