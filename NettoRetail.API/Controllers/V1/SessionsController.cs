using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Sessions.Commands;
using NettoRetail.Application.CQRS.Sessions.Queries;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Session;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class SessionsController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActiveSessions(CancellationToken cancellationToken)
    {
        var id = HttpContext.GetUserId();
        var getActiveSessionsQuery = new GetActiveSessionsQuery(id);
        var result = await mediator.Send(getActiveSessionsQuery, cancellationToken);
        var response = new ApiResponse<IEnumerable<SessionResponse>>(result);

        return Ok(response);
    }

    [Authorize]
    [HttpPost("terminate-session")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TerminateSession([FromHeader(Name = "Authorization")] string authHeader, SessionTerminateModel model, CancellationToken cancellationToken)
    {
        var command = new TerminateSessionCommand(model, authHeader);

        await mediator.Send(command, cancellationToken);

        return Ok();
    }

    [Authorize]
    [HttpPost("terminate-all-sessions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TerminateAllSessions([FromHeader(Name = "Authorization")] string authHeader, CancellationToken cancellationToken)
    {
        var id = HttpContext.GetUserId();

        var command = new TerminateAllSessionsCommand(id, authHeader);

        await mediator.Send(command, cancellationToken);

        return Ok();
    }
}