using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Sessions.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.API.Controllers.V1
{

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

            var currentSessionToken = JwtUtils.ExtractTokenFromAuthorizationHeader(HttpContext)
                ?? throw new NotFoundException("Current session unavailable");

            var getActiveSessionsQuery = new GetActiveSessionsQuery(id, currentSessionToken);
            var result = await mediator.Send(getActiveSessionsQuery, cancellationToken);
            var response = new ApiResponse<IEnumerable<SessionResponse>>(result);

            return Ok(response);
        }
    }
}
