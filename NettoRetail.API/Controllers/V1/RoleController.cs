using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Users.Queries.Role;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
[Route("api/v{v:ApiVersion}/[controller]")]
public class RoleController(IMediator mediator) : ControllerBase
{
    /*
    * API endpoint with 4 optional parameters: Id, Name, page, and pageSize.
    *
    * - If Id is not provided, no filtering by role name is applied.
    * - If name is not provided, no filtering by role name is applied.
    * - If page or pageSize are not provided, default page and default pageSize are applied 
    */

    [Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
    [HttpGet("By-Id-or-Name")]
    public async Task<IActionResult> GetRolesByNameOrId([FromQuery] RoleSearchModel model, CancellationToken cancellationToken)
    {
        var query = new GetRolesQueryPaginated(model);
        var result = await mediator.Send(query, cancellationToken);
        var response = new ApiResponse<PaginatedResponse<RoleResponse>>(result);
        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
    [HttpGet("Get-All-Roles")]
    public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
    {
        var query = new GetRolesQuery();
        var result = await mediator.Send(query, cancellationToken);
        var response = new ApiResponse<List<RoleResponse>>(result);
        return Ok(response);
    }

}