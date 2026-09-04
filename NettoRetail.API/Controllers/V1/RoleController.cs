using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Roles.Queries;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
[Route("api/v{v:ApiVersion}/[controller]")]
public class RoleController(IMediator mediator) : ControllerBase
{
    [Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<RoleResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllRoles([FromQuery] PaginationRequest paginationRequest, CancellationToken cancellationToken)
    {
        var query = new GetRolesQuery(paginationRequest);
        var result = await mediator.Send(query, cancellationToken);
        var response = new ApiResponse<PaginatedResponse<RoleResponse>>(result);
        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
    [HttpGet("{id::int}")]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRolesById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var query = new GetRolesQueryById(id);
        var result = await mediator.Send(query, cancellationToken);
        var response = new ApiResponse<RoleResponse>(result);
        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
    [HttpGet("{name}")]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRolesByName(string name, CancellationToken cancellationToken)
    {
        var query = new GetRolesQueryByName(name);
        var result = await mediator.Send(query, cancellationToken);
        var response = new ApiResponse<RoleResponse>(result);
        return Ok(response);
    }
}