using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Sessions.Commands;
using NettoRetail.Application.CQRS.Users.Commands.Admin;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Staff;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]

[Route("/api/v{version:apiVersion}/[controller]")]
public class AdminsController(IMediator mediator) : ControllerBase
{
    [Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
    [HttpDelete]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var command = new DeleteAdminCommand(userId, request.Password);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}