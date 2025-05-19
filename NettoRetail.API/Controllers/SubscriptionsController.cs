using Asp.Versioning;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.Application.CQRS.Subscriptions.Commands;
using NettoRetail.Application.Models.Request.Subscription;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class SubscriptionsController(IMediator mediator) : ControllerBase
{
    //[Authorize]
    //[HttpGet("plans")]
    //public async Task<IActionResult> GetPlans()
    //{
    //    var result = await mediator.Send(new GetSubscriptionsPlansQuery());
    //    return Ok(result);
    //}

    [Authorize]
    [HttpPut("api/shops/{shopId}/subscription")]
    public async Task<IActionResult> ChangeSubscription(int shopId, [FromBody] ChangeSubscriptionRequest request)
    {
        var command = new ChangeSubscriptionCommand(shopId, request.NewPlan, request.PaymentMethodId);
        await mediator.Send(command);
        return NoContent();
    }

}