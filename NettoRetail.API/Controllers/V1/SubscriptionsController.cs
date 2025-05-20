using Asp.Versioning;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.Subscriptions.Commands;
using NettoRetail.Application.Models.Request.Subscription;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/subscription")]
public class SubscriptionsController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPut("{shopId}")]
    public async Task<IActionResult> ChangeSubscription(int shopId, [FromBody] ChangeSubscriptionRequest request)
    {
        var userId = HttpContext.GetUserId();
        var command = new ChangeSubscriptionCommand(shopId, userId, request.NewPlan, request.PaymentMethodId);
        await mediator.Send(command);
        return NoContent();
    }
}