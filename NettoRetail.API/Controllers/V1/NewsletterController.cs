using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.Application.CQRS.Newsletters.Commands;
using NettoRetail.Application.Models.Email;
using System.ComponentModel.DataAnnotations;

namespace NettoRetail.API.Controllers.V1;

[ApiVersion(1)]
[ApiController]
[Route("api/v1/[controller]")]
public class NewsletterController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Subscribes the given email to the newsletter.
    /// </summary>
    /// <param name="model">Email input</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Status result</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostNewsletter([FromBody] EmailModelRequest model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                                   .SelectMany(v => v.Errors)
                                   .Select(e => e.ErrorMessage)
                                   .ToList();

            return BadRequest(new
            {
                success = false,
                message = "Validation failed",
                errors
            });
        }

        try
        {
            var command = new PostNewsletterCommand(model);
            await mediator.Send(command, cancellationToken);

            return Ok(new
            {
                success = true,
                message = "Successfully subscribed"
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An unexpected error occurred"
            });
        }
    }
}
