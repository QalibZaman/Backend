using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Delivery.Commands;
using NettoRetail.Application.CQRS.Delivery.Queries;
using NettoRetail.Application.Models.Delivery;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class DeliveryController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [Authorize]
    [HttpGet("current")]
    [ProducesResponseType(typeof(ApiResponse<DeliveryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> GetDelivery(
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var shopId = HttpContext.GetUserShopId();

        var query = new GetCurrentDeliveryQuery(userId, shopId);
        var response = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<DeliveryResponse>(response));
    }

    [Authorize]
    [HttpPatch("current/method")]
    [ProducesResponseType(typeof(ApiResponse<DeliveryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> SetDeliveryMethod(
        [FromBody] SetDeliveryMethodRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var shopId = HttpContext.GetUserShopId();

        var command = new SetDeliveryMethodCommand(userId, shopId, request);
        var response = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<DeliveryResponse>(response));
    }

    [Authorize]
    [HttpPatch("current/address")]
    [ProducesResponseType(typeof(ApiResponse<DeliveryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> SetAddress(
        [FromBody] SetDeliveryAddressRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var shopId = HttpContext.GetUserShopId();

        var command = new SetDeliveryAddressCommand(userId, shopId, request);
        var response = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<DeliveryResponse>(response));
    }
}
