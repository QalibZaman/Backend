using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Sessions.Commands;
using NettoRetail.Application.CQRS.Users.Commands.Admin;
using NettoRetail.Application.CQRS.Users.Queries.Account;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Account;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class AdminsController(IMediator mediator) : ControllerBase
{
    [HttpPost("sign-up")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> SignUp(AdminSignUpModel model)
    {
        var adminCreationCommand = new CreateAdminCommand(model);

        var data = await mediator.Send(adminCreationCommand);

        // Creating a session
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var sessionCreateCommand = new CreateSessionCommand(ipAddress, Request.Headers["User-Agent"].ToString(), data.AccessToken, data.RefreshToken);
        await mediator.Send(sessionCreateCommand);

        var response = new ApiResponse<TokenResponse>(data);

        return Created("", response);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AccountDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> GetStaffById(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var query = new GetAccountDetailsByIdQuery(userId);
        var data = await mediator.Send(query, cancellationToken);

        var response = new ApiResponse<AccountDetailsResponse>(data);
        return Ok(response);
    }
}
