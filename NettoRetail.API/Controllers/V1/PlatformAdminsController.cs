using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Attributes;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.Users.Commands.Admin;
using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1
{
    [ApiController]
    [ApiVersion(1)]
    [Route("/api/v{version:apiVersion}/platform-admins")]
    public class PlatformAdminsController(IMediator mediator) : ControllerBase
    {
        [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
        [Authorize]
        [HttpPost("{id:guid}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActivatePlatformAdmin(Guid id)
        {
            var currentAdminId = HttpContext.GetUserId();

            var activatePlatformAdminCommand = new ActivatePlatfromAdminCommand(id, currentAdminId);
            await mediator.Send(activatePlatformAdminCommand);

            return NoContent();
        }

        [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
        [Authorize]
        [HttpPost("{id:guid}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeactivatePlatformAdmin(Guid id)
        {
            var currentAdminId = HttpContext.GetUserId();

            var deactivatePlatfromAdminCommand = new DeactivatePlatfromAdminCommand(id, currentAdminId);
            await mediator.Send(deactivatePlatfromAdminCommand);

            return NoContent();
        }
    }
}
