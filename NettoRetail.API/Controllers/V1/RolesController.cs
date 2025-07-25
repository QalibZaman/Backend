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
public class RolesController : ControllerBase
{
    private readonly IMediator mediator;

    public RolesController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission("System settings")]
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateRoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<CreateRoleResponse>>> AddRole(
        [FromBody] CreateRoleRequest model,
        CancellationToken cancellationToken)
    {
        var command = model.Adapt<CreateRoleCommand>() with
        {
            ShopId = HttpContext.GetUserShopId()
        };

        var role = await mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<CreateRoleResponse>(role));
    }

    [Authorize]
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GetAllRolesResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<GetAllRolesResponse>>>> GetAllRoles(
        CancellationToken cancellationToken)
    {
        var query = new GetAllRolesQuery(HttpContext.GetUserShopId());
        var data = await mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<IEnumerable<GetAllRolesResponse>>(data));
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteRoleCommand(id, HttpContext.GetUserShopId());
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<RoleResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<RoleResponse>>>> GetRoles(
        [FromQuery] PaginationRequest model,
        CancellationToken cancellationToken)
    {
        var query = new GetRolesQuery(model, HttpContext.GetUserShopId());
        var response = await mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<PaginatedResponse<RoleResponse>>(response));
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditRole(
        int id,
        [FromBody] UpdateRoleRequest model,
        CancellationToken cancellationToken)
    {
        var command = new RoleUpdateCommand(id, HttpContext.GetUserShopId(), model);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
}
