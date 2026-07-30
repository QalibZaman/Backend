using Microsoft.AspNetCore.Authorization;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.Newsletters.Commands;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:ApiVersion}/[controller]")]
public class StaffController(
    IMediator mediator) : ControllerBase
{
    [HttpPost("subscribe-to-newsletter")]
    //[Authorize(Policy = PolicyNames.AdminOnlyPolicy)]
    public async Task<IActionResult> SubscribeToNewsletter(CancellationToken cancellationToken)
    {
        //var accountId = HttpContext.GetUserId();
        var accountId = new Guid("5ba00984-d376-45f6-86ed-bc1430be2016");
        var command = new SubscribeToNewsletterCommand(accountId);
        await mediator.Send(command, cancellationToken);
        return Ok();
    }
}