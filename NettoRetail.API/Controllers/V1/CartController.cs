using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Cart.Commands;
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

        [HttpPost("add")]
        [ProducesResponseType(typeof(ApiResponse<AddToCartResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddItem([FromQuery] Guid inventoryItemId, CancellationToken cancellationToken)
        {
            var customerId = HttpContext.GetUserId();
            var command = new AddItemToCartCommand(
                InventoryItemId: inventoryItemId,
                CustomerId: customerId);

            var addToCartResponse = await _mediator.Send(command, cancellationToken);

            return Created(string.Empty, new ApiResponse<AddToCartResponse>(addToCartResponse));
        }
    }
}
