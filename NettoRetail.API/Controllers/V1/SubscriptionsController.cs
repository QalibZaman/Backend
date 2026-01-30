using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Attributes;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Subscription.Queries;
using NettoRetail.Application.CQRS.Subscriptions.Commands;
using NettoRetail.Application.Models.Request.Subscription;
using NettoRetail.Application.Models.Subscriptions;
using NettoRetail.Shared.Constants;
using StackExchange.Redis;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/subscription")]
public class SubscriptionsController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPut("shop/{shopId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeSubscription(int shopId, [FromBody] ChangeSubscriptionRequest request)
    {
        var userId = HttpContext.GetUserId();
        var command = new ChangeSubscriptionCommand(shopId, userId, request.NewPlan, request.PaymentMethodId);
        await mediator.Send(command);
        return NoContent();
    }

    [Authorize]
    [HttpPut("plan/{planId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [ProducesResponseType(typeof(ApiResponse<List<SubscriptionPlanResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> GetPlans()
    {
        var shopId = HttpContext.GetUserShopId();
        var result = await mediator.Send(new GetSubscriptionsPlansQuery(shopId));
        var response = new ApiResponse<List<SubscriptionPlanResponse>>(result);
        return Ok(response);
    }
}