using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
[Route("api/v{v:ApiVersion}/[controller]")]
public class RoleController(IMediator mediator) : ControllerBase
{
    [Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRole(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteRoleCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}