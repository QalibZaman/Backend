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
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
[ApiController]
public class RolesController(IMediator mediator) : ControllerBase
{
    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission("System settings")]
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateRoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateRoleResponse>>> AddRole([FromBody] CreateRoleRequest model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var command = model.Adapt<CreateRoleCommand>() with { ShopId = shopId };
        var result = await mediator.Send(command, cancellationToken);
        var response = new ApiResponse<CreateRoleResponse>(result);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GetAllRolesResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<GetAllRolesResponse>>>> GetAllRoles(CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var query = new GetAllRolesQuery(shopId);
        var result = await mediator.Send(query, cancellationToken);
        var response = new ApiResponse<IEnumerable<GetAllRolesResponse>>(result);
        return Ok(response);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRole(int id, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var command = new DeleteRoleCommand(id, shopId);
        await mediator.Send(command, cancellationToken);
        var response = new ApiResponse<string>("Role deleted successfully");
        return Ok(response);
    }

    [Authorize]
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

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EditRole(int id, [FromBody] UpdateRoleRequest model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var roleUpdateCommand = new RoleUpdateCommand(id, shopId, model);
        await mediator.Send(roleUpdateCommand, cancellationToken);
        var response = new ApiResponse<string>("Role updated successfully");
        return Ok(response);
    }
}
