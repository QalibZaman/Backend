using Asp.Versioning;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.Subscription.Queries;
using NettoRetail.Application.CQRS.Subscriptions.Commands;
using NettoRetail.Application.Models.Request.Subscription;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/subscription")]
public class SubscriptionsController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPut("shop/{shopId}")]
    public async Task<IActionResult> ChangeSubscription(int shopId, [FromBody] ChangeSubscriptionRequest request)
    {
        var userId = HttpContext.GetUserId();
        var command = new ChangeSubscriptionCommand(shopId, userId, request.NewPlan, request.PaymentMethodId);
        await mediator.Send(command);
        return NoContent();
    }

    [Authorize]
    [HttpPut("plan/{planId}")]
    public async Task<IActionResult> UpdateSubscription(int planId, [FromBody] UpdateSubscriptionPlanRequest request)
    {
        var userId = HttpContext.GetUserId();
        var command = new UpdateSubscriptionPlanCommand(
        planId,
        userId,
        request.Name,
        request.Price,
        request.Description,
        request.Motto,
        request.FeatureIds);
        await mediator.Send(command);
        return NoContent();
    }

    [Authorize]
    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans()
    {
        var result = await mediator.Send(new GetSubscriptionsPlansQuery());
        return Ok(result);
    }
}