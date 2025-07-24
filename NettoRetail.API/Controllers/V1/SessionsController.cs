using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Sessions.Queries;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class SessionsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator mediator = mediator;

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetActiveSessions(CancellationToken cancellationToken)
    {
        Guid userId;
        try
        {
            userId = HttpContext.GetUserId();
        }
        catch (Exception)
        {
            return NotFound(new ApiResponse("User context is unavailable"));
        }

        var getActiveSessionsQuery = new GetActiveSessionsQuery(userId);
        var result = await mediator.Send(getActiveSessionsQuery, cancellationToken);
        return Ok(new ApiResponse<IEnumerable<SessionResponse>>(result));
    }
}