using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Orders.Commands;
using NettoRetail.Application.CQRS.Orders.Queries;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("{orderId:int}/products")]
    [ProducesResponseType(typeof(ApiResponse<OrderProductsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderProducts(int orderId, CancellationToken cancellationToken)
    {
        var orderCreationCommand = new GetOrderProductsQuery(orderId);

        var data = await mediator.Send(orderCreationCommand, cancellationToken);
        var response = new ApiResponse<OrderProductsResponse>(data);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<OrderListResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFilteredOrders([FromQuery] GetFilteredOrdersModel model, [FromQuery] string? searchTerm, [FromQuery] PaginationRequest pageModel, CancellationToken cancellationToken)
    {
        var userShopId = HttpContext.GetUserShopId();

        var query = new GetFilteredOrdersQuery(model, userShopId, searchTerm, pageModel);

        var data = await mediator.Send(query, cancellationToken);

        var response = new ApiResponse<PaginatedResponse<OrderListResponse>>(data);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<ICollection<SearchOrderResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSearchOptions([FromQuery] SearchFieldModel search, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var query = new GetSearchOptionsQuery(search, shopId);
        var data = await mediator.Send(query, cancellationToken);
        var response = new ApiResponse<ICollection<SearchOrderResponse>>(data);
        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPatch("{orderId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeOrderStatus([FromQuery] UpdateOrderRequest model, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var command = new UpdateOrderStatusCommand(model, userId);
        var data = await mediator.Send(command, cancellationToken);
        var response = new ApiResponse<OrderResponse>(data);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("print/{orderId}")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderPrintingDetails(int orderId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOrderDetailsForPrintingQuery(orderId), cancellationToken);

        result.Stream.Position = 0;

        return new FileStreamResult(result.Stream, result.ContentType)
        {
            FileDownloadName = result.FileName
        };
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("history/{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderHistoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrdersHistory(Guid customerId, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var getOrderHistoryQuery = new GetOrderHistoryQuery(customerId, shopId);
        var data = await mediator.Send(getOrderHistoryQuery, cancellationToken);
        var response = new ApiResponse<IEnumerable<OrderHistoryResponse>>(data);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("{orderId:int}/information")]
    [ProducesResponseType(typeof(ApiResponse<OrderGeneralInfoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderGeneralInfo(int orderId, CancellationToken cancellationToken)
    {
        var getOrderInfoQuery = new GetOrderGeneralInfoQuery(orderId);
        var data = await mediator.Send(getOrderInfoQuery, cancellationToken);
        var response = new ApiResponse<OrderGeneralInfoResponse>(data);

        return Ok(response);
    }
}
