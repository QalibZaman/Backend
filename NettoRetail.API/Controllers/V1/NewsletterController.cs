using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.Application.CQRS.Newsletters.Commands;
using NettoRetail.Application.Models.Email;

namespace NettoRetail.API.Controllers.V1;

[ApiVersion(1)]
[ApiController]
[Route("api/v1/[controller]")]
public class NewsletterController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostNewsletter([FromBody] EmailModelRequest model, CancellationToken cancellationToken)
    {
        var command = new PostNewsletterCommand(model);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
}
