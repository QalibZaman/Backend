using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Orders.Commands;
using NettoRetail.Application.CQRS.Orders.Queries;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Request.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetOrderDetails(int orderId, CancellationToken cancellationToken)
    {
        var orderCreationCommand = new GetOrderDetailsQuery(orderId);

        var data = await mediator.Send(orderCreationCommand, cancellationToken);
        var response = new ApiResponse<OrderResponse>(data);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderModel model, CancellationToken cancellationToken)
    {
        var userShopId = HttpContext.GetUserShopId();
        var userId = HttpContext.GetUserId();
        var orderCreationCommand = new CreateOrderCommand(model, userShopId, userId);

        var data = await mediator.Send(orderCreationCommand, cancellationToken);
        var response = new ApiResponse<OrderResponse>(data);

        return Created(string.Empty, response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPatch("{orderId:int}")]
    public async Task<IActionResult> UpdateOrder(int orderId, [FromBody] UpdateOrderModel model, CancellationToken cancellationToken)
    {
        var userShopId = HttpContext.GetUserShopId();
        var userId = HttpContext.GetUserId();
        var command = new UpdateOrderCommand(model, userShopId, userId, orderId);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost("status")]
    public async Task<IActionResult> ChangeOrderStatus([FromQuery] UpdateOrderRequest model, CancellationToken cancellationToken)
    {
        var command = new UpdateOrderStatusCommand(model);
        var data = await mediator.Send(command, cancellationToken);
        var response = new ApiResponse<OrderResponse>(data);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost("history/{customerId:guid}")]
    public async Task<IActionResult> GetOrdersHistory(Guid customerId, CancellationToken cancellationToken)
    {
        var getOrderHistoryQuery = new GetOrderHistoryQuery(customerId);
        var data = await mediator.Send(getOrderHistoryQuery, cancellationToken);
        var response = new ApiResponse<IEnumerable<OrderHistoryResponse>>(data);

        return Ok(response);
    }
}