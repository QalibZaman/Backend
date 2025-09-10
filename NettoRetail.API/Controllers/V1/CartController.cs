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
using NettoRetail.DataAccess.Migrations;

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

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CartItemResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
        {
            var customerId = HttpContext.GetUserId();
            var cartItems = await _mediator.Send(new GetCartQuery(customerId), cancellationToken);
            return Ok(new ApiResponse<List<CartItemResponse>>(cartItems));
        }

        [HttpPost("add")]
        [ProducesResponseType(typeof(ApiResponse<AddToCartResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request, CancellationToken cancellationToken)
        {
            var customerId = HttpContext.GetUserId();
            var command = new AddItemToCartCommand(
                ProductUpc: request.ProductUpc,
                ProductName: request.ProductName,
                Price: request.Price,
                Quantity: request.Quantity,
                PromotionPrice: request.PromotionPrice,
                CustomerId: customerId
            );

            var addToCartResponse = await _mediator.Send(command, cancellationToken);
            return Created(string.Empty, new ApiResponse<AddToCartResponse>(addToCartResponse));
        }

        [HttpGet("{productUpc}/availability")]
        [ProducesResponseType(typeof(ApiResponse<ProductAvailabilityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckAvailability(long productUpc, CancellationToken cancellationToken)
        {
            if (productUpc <= 0)
            {
                var invalidResponse = new ProductAvailabilityResponse
                {
                    Success = false,
                    InStock = false,
                    Message = "Invalid product UPC"
                };
                return Ok(new ApiResponse<ProductAvailabilityResponse>(invalidResponse));
            }

            var response = await _mediator.Send(new CheckProductAvailabilityQuery(productUpc), cancellationToken);
            return Ok(new ApiResponse<ProductAvailabilityResponse>(response));
        }
    }
}
