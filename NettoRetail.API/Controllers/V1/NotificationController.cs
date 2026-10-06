using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Notifications.Command;
using NettoRetail.Application.CQRS.Notifications.Queries;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Notification;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Authorize]
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

    [HttpGet("get-all")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<NotificationResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination)
    {
        var staffId = HttpContext.GetUserId();
        var data = new GetAllNotificationsQuery(pagination, staffId);
        var response = await mediator.Send(data);
        return Ok(new ApiResponse<PaginatedResponse<NotificationResponse>>(response));
    }

    [HttpPost("add")]
    [Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddNotification([FromBody] CreateNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateNotificationCommand(request);
        var result = await mediator.Send(command, cancellationToken);
        var responce = new ApiResponse<AddNotificationResponce>(result);
        return Ok(responce);
    }

    [HttpPost("NotificationPreference")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetNotificationPreference(
        UpdateNotificationPreferenceRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var command = new SetNotificationPreferenceCommand(request, userId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("NotificationPreferences")]
    [Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> NotificationPreferences(CancellationToken cancellationToken)
    {
        var staffId = HttpContext.GetUserId();
        var query = new GetAllNotificationPreferencesQuery(staffId);
        var response = await mediator.Send(query, cancellationToken);
        return Ok(response);
    }
}