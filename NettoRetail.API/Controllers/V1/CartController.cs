using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Cart.Commands;
using NettoRetail.Application.Models.Request.Cart;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.API.Controllers.V1
{
    [ApiController]
    [ApiVersion(1)]
    [Route("api/v{v:ApiVersion}/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CartController(IMediator mediator)
        {
            _mediator = mediator;
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
    }
}
