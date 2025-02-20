using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NettoHealth.Application.CQRS.Users.Commands;
using NettoHealth.Application.Models.Entities;
using NettoHealth.Application.Models.Request;

namespace NettoHealth.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class HealthcareProviderController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterHealthcareProvider(HealthcareProviderRegistrationModel model)
    {
        var healthcareProviderCreationCommand = new CreateHealthcareProviderCommand(model);
        await mediator.Send(healthcareProviderCreationCommand);

        return NoContent();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var loginCommand = new LoginCommand(model);
        var response = await mediator.Send(loginCommand);

        return Ok(response.Data);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var response = await mediator.Send(command);

        return Ok(response.Data);
    }
}