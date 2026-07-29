using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Sessions.Commands;
using NettoRetail.Application.CQRS.Users.Commands.Auth;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Email;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:ApiVersion}/[controller]")]
public class StaffController(IMediator mediator) : ControllerBase
{
    [HttpPost("log-in")]
    public async Task<IActionResult> LogIn([FromBody] LoginModel model)
    {
        var loginCommand = new LoginCommand(model);
        var data = await mediator.Send(loginCommand);
        var response = new ApiResponse<TokenResponse>(data);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var sessionCreateCommand = new CreateSessionCommand(
            ipAddress,
            Request.Headers["User-Agent"].ToString(),
            data.AccessToken,
            data.RefreshToken,
            UserTypeEnum.Admin);

        await mediator.Send(sessionCreateCommand);

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
}