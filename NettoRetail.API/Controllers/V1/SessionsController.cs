using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Sessions.Queries;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class SessionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetActiveSessions(Guid id)
    {
        var jwt = Request.Headers["Authorization"].ToString();
        var getActiveSessionsQuery = new GetActiveSessionsQuery(id);
        var result = await mediator.Send(getActiveSessionsQuery);
        var response = new ApiResponse<IEnumerable<SessionResponse>>(result);

        return Ok(response);
    }
}