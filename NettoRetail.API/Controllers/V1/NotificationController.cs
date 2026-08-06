using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Notifications.Command;
using NettoRetail.Application.CQRS.Notifications.Queries;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
[Route("api/v{v:ApiVersion}/[controller]")]
public class NotificationController(IMediator mediator) : ControllerBase
{
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var staffId = HttpContext.GetUserId();
        var data = new MarkAsReadCommand(id, staffId);
        await mediator.Send(data);
        return NoContent();
    }

    [HttpPost("{id:guid}/unread")]
    public async Task<IActionResult> MarkAsUnread(Guid id)
    {
        var staffId = HttpContext.GetUserId();
        var data = new MarkAsUneadCommand(id, staffId);
        await mediator.Send(data);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> ReadAll()
    {
        var staffId = HttpContext.GetUserId();
        var data = new ReadAllNotificationsCommand(staffId);
        await mediator.Send(data);
        return NoContent();
    }

    [HttpPost("get-all")]
    [ProducesResponseType(typeof(ApiResponse<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var staffId = HttpContext.GetUserId();
        var data = new GetAllNotificationsQuery(staffId);
        var response = await mediator.Send(data);
        return Ok(new ApiResponse<PaginatedResponse<NotificationResponse>>(response));
    }
}