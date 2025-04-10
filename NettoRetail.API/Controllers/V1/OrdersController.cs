using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Orders.Commands;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Response;
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
}