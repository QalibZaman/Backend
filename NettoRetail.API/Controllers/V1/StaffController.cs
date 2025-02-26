using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Users.Commands;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;

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

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var data = await mediator.Send(command);
        var response = new ApiResponse<TokenResponse>(data);

        return Ok(response);
    }
}