using System.Security.Claims;
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
    [HttpPost("status")]
    public async Task<IActionResult> ChangeOrderStatus([FromQuery] UpdateOrderRequest model, CancellationToken cancellationToken)
    {
        var command = new UpdateOrderStatusCommand(model);
        var data = await mediator.Send(command, cancellationToken);
        var response = new ApiResponse<OrderResponse>(data);

        return Ok(response);
    }

    [HttpGet("print/{orderId}")]
    public async Task<IActionResult> GetOrderPrintingDetails(int orderId, CancellationToken cancellationToken)
    {
        var command = new GetOrderDetailsForPrintingQuery(orderId);
        var data = await mediator.Send(command, cancellationToken);
        var response = new ApiResponse<OrderPrintingResponse>(data);

        return Ok(response);
    }
}