using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.Application.CQRS.Users.Commands;
using NettoRetail.Application.Models.Admin;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class AdminsController(IMediator mediator) : ControllerBase
{
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp(AdminSignUpModel model)
    {
        var adminCreationCommand = new CreateAdminCommand(model);

        await mediator.Send(adminCreationCommand);

        return NoContent();
    }
}