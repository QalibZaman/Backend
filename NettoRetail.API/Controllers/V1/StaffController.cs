using Microsoft.AspNetCore.Authorization;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.Newsletters.Commands;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:ApiVersion}/[controller]")]
public class StaffController(
    IMediator mediator) : ControllerBase
{
    [HttpPost("subscribe-to-newsletter")]
    [Authorize]
    public async Task<IActionResult> SubscribeToNewsletter(CancellationToken cancellationToken)
    {
        var accountId = HttpContext.GetUserId();
        var command = new SubscribeToNewsletterCommand(accountId);
        await mediator.Send(command, cancellationToken);
        return Ok();
    }

    [HttpPost("unsubscribe-to-newsletter")]
    [Authorize]
    public async Task<IActionResult> UnsubscribeToNewsletter(CancellationToken cancellationToken)
    {
        var accountId = HttpContext.GetUserId();
        var command = new UnsubscribeToNewsletterCommand(accountId);
        await mediator.Send(command, cancellationToken);
        return Ok();
    }
}