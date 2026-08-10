using Asp.Versioning;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Sessions.Commands;
using NettoRetail.Application.CQRS.Users.Commands.Admin;
using NettoRetail.Application.CQRS.Users.Commands.Auth;
using NettoRetail.Application.CQRS.Users.Queries.Account;
using NettoRetail.Application.Models.Email;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Utilities;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:ApiVersion}/[controller]")]
public class StaffController(
    IMediator mediator,
    IMapper mapper) : ControllerBase
{
    [Authorize]
    [HttpGet("personal-information")]
    public async Task<IActionResult> GetPersonalInformation(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        var query = new GetPersonalInfoQuery(userId);
        var data = await mediator.Send(query, cancellationToken);

        var response = new ApiResponse<AccountDetailsResponse>(data);
        return Ok(response);
    }

    [HttpPost("locked-account-activation")]
    public async Task<IActionResult> ResendActivationEmail([FromBody] ActivationEmailAttemptRequest model)
    {
        var emailCommand = new ResendActivationEmailCommand(model);
        await mediator.Send(emailCommand);

        return Ok(new
        {
            message = "If an account with that email exists, an activation email has been sent.",
        });
    }

    [HttpPost("log-out")]
    [Authorize]
    public async Task<IActionResult> LogOut()
    {
        var token = JwtUtils.ExtractTokenFromAuthorizationHeader(HttpContext);
        var userId = HttpContext.GetUserId();

        var logoutCommand = new LogoutCommand(token!, userId);
        await mediator.Send(logoutCommand);

        return NoContent();
    }
    [HttpGet("activate")]
    public async Task<IActionResult> Activate([FromQuery] string token)
    {
        var activateLockedAccountCommand = new ActivateLockedAccountCommand(token);
        await mediator.Send(activateLockedAccountCommand);
        return Ok();
    }
}