using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.Subscriptions.Commands;
using NettoRetail.Application.Models.Request;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class SubscriptionController(IMediator mediator) : ControllerBase
{
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var userId = HttpContext.GetUserId();

        var command = new SubscribeToThePlanCommand(shopId, userId,  model);
        await mediator.Send(command, cancellationToken);
        return Ok();
    }
}