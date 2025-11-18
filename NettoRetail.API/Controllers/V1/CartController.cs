using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Cart.Commands;
using NettoRetail.Application.CQRS.Cart.Queries;
using NettoRetail.Application.Models.Request.Cart;
using NettoRetail.Application.Models.Response;
using Z.BulkOperations;

namespace NettoRetail.API.Controllers.V1
{
    [ApiController]
    [ApiVersion(1)]
    [Route("api/v{v:ApiVersion}/[controller]")]
    [Authorize]
    public class CartController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet("items")]
        [ProducesResponseType(typeof(ApiResponse<GetCartItemsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetCartItems(CancellationToken cancellationToken)
        {
            var customerId = HttpContext.User.GetUserId();
            var query = new GetCartItemsQuery(customerId);

            var result = await _mediator.Send(query);

            return Ok(new ApiResponse<GetCartItemsResponse>(result));
        }

        [HttpPost("items")]
        [ProducesResponseType(typeof(ApiResponse<AddToCartResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddItem([FromBody] Guid inventoryItemId, CancellationToken cancellationToken)
        {
            var customerId = HttpContext.GetUserId();
            var command = new AddItemToCartCommand(
                InventoryItemId: inventoryItemId,
                CustomerId: customerId);

            var addToCartResponse = await _mediator.Send(command, cancellationToken);

            return Created(string.Empty, new ApiResponse<AddToCartResponse>(addToCartResponse));
        }

        [HttpDelete("items")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClearItems(CancellationToken cancellationToken)
        {
            var customerId = HttpContext.GetUserId();
            var command = new ClearItemsFromCartCommand(customerId);

            await _mediator.Send(command, cancellationToken);

            return NoContent();
        }

        [HttpDelete("items/{itemId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteItem(Guid itemId, CancellationToken cancellationToken)
        {
            var customerId = HttpContext.GetUserId();
            var command = new DeleteItemFromCartCommand(itemId, customerId);

            await _mediator.Send(command, cancellationToken);

            return NoContent();
        }

        [HttpPut("items/restore")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CartItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RestoreAllItems(CancellationToken cancellationToken)
        {
            var customerId = HttpContext.GetUserId();
            var command = new RestoreAllCartItemsCommand(customerId);

            var data = await _mediator.Send(command, cancellationToken);

            var response = new ApiResponse<IEnumerable<CartItemResponse>>(data);

            return Ok(response);
        }

        [HttpPut("items/{itemId:guid}/restore")]
        [ProducesResponseType(typeof(ApiResponse<CartItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RestoreItem(Guid itemId, CancellationToken cancellationToken)
        {
            var customerId = HttpContext.GetUserId();
            var command = new RestoreCartItemCommand(itemId, customerId);

            var data = await _mediator.Send(command, cancellationToken);

            var response = new ApiResponse<CartItemResponse>(data);

            return Ok(response);
        }

        [HttpPut("items/{cartItemId:guid}/quantity")]
        [ProducesResponseType(typeof(ApiResponse<CartItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCartItemQuantity(Guid cartItemId, [FromBody] UpdateCartItemQuantityRequest quantity, CancellationToken cancellationToken)
        {
            var customerId = HttpContext.GetUserId();
            var command = new UpdateCartItemQuantityCommand(
                CartItemId: cartItemId,
                Quantity: quantity.Quantity,
                CustomerId: customerId);

            var response = await _mediator.Send(command, cancellationToken);

            return Ok(new ApiResponse<CartItemResponse>(response));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<CartSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var customerId = HttpContext.GetUserId();
            var query = new GetCartSummaryQuery(customerId);

            var summary = await _mediator.Send(query, cancellationToken);

            return Ok(new ApiResponse<CartSummaryResponse>(summary));
        }
    }
}
