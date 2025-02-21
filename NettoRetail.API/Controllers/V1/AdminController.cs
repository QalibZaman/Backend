using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.Application.CQRS.Users.Commands;
using NettoRetail.Application.Models.Entities;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class AdminController(IMediator mediator) : ControllerBase
{
    [HttpPost("sign-up")]
    public async Task SignUp(AdminSignUpModel model)
    {
        var adminCreationCommand = new CreateAdminCommand(model);

        await mediator.Send(adminCreationCommand);
    }
}