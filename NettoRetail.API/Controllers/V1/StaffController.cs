using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Sessions.Commands;
using NettoRetail.Application.CQRS.Users.Commands;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Session;

namespace NettoRetail.API.Controllers.V1;
[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:ApiVersion}/[controller]")]
public class StaffController(
    IMediator mediator) : ControllerBase
{
    [HttpPost("log-in")]
    public async Task<IActionResult> LogIn([FromBody] LoginModel model)
    {
        var loginCommand = new LoginCommand(model);
        var data = await mediator.Send(loginCommand);

        // Creating a session
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var sessionCreateCommand = new CreateSessionCommand(ipAddress, Request.Headers["User-Agent"].ToString(), data.AccessToken, data.RefreshToken);
        await mediator.Send(sessionCreateCommand);

        var response = new ApiResponse<TokenResponse>(data);

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var data = await mediator.Send(command);

        var sessionUpdateCommand = new UpdateSessionCommand(new SessionUpdateModel
        {
            CurrentJwt = data.AccessToken,
            LastActivityTime = DateTime.UtcNow,
            CurrentRefreshToken = data.RefreshToken,
            PreviousRefreshToken = request.RefreshToken,
        });
        await mediator.Send(sessionUpdateCommand);

        var response = new ApiResponse<TokenResponse>(data);

        return Ok(response);
    }

    [HttpPost("log-out")]
    [Authorize]
    public async Task<IActionResult> LogOut()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        var token = authHeader.Replace("Bearer ", "");

        var logoutCommand = new LogoutCommand(token);
        await mediator.Send(logoutCommand);

        return NoContent();
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest model)
    {
        var userId = HttpContext.GetUserId();
        var changePasswordCommand = new ChangePasswordCommand(userId, model);

        await mediator.Send(changePasswordCommand);

        return NoContent();
    }
}
