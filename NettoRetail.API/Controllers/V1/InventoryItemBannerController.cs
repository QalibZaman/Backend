using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.ProductBanners.Commands;
using NettoRetail.Application.CQRS.ProductBanners.Queries;
using NettoRetail.Application.Models.Request.ProductBanner;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.API.Controllers.V1
{
    [ApiController]
    [ApiVersion(1)]
    [Route("api/v{v:ApiVersion}/[controller]")]
    public class InventoryItemBannerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InventoryItemBannerController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet("{inventoryItemId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<GetInventoryItemBannerResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize]
        public async Task<IActionResult> GetBanners(Guid inventoryItemId, CancellationToken cancellationToken)
        {
            var query = new GetInventoryItemBannersQuery(inventoryItemId);
            var banners = await _mediator.Send(query, cancellationToken);
            return Ok(new ApiResponse<List<GetInventoryItemBannerResponse>>(banners));
        }


        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<List<GetInventoryItemBannerResponse>>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize]
        public async Task<IActionResult> AddBanners([FromForm] AddInventoryItemBannerRequest request, CancellationToken cancellationToken)
        {
            var command = new AddInventoryItemBannersCommand(request.InventoryItemId, request.Files.ToArray());
            var result = await _mediator.Send(command, cancellationToken);
            return Created(string.Empty, new ApiResponse<List<GetInventoryItemBannerResponse>>(result));
        }


        [HttpDelete("{bannerId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize]
        public async Task<IActionResult> DeleteBanner(Guid bannerId, CancellationToken cancellationToken)
        {
            var command = new DeleteInventoryItemBannerCommand(bannerId);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }


        [HttpPatch("{bannerId:guid}/order")]
        [ProducesResponseType(typeof(ApiResponse<List<GetInventoryItemBannerResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize]
        public async Task<IActionResult> UpdateBannerOrder(Guid bannerId, [FromQuery] int newOrderIndex, CancellationToken cancellationToken)
        {
            var command = new ReorderInventoryItemBannerOrderCommand(bannerId, newOrderIndex);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(new ApiResponse<List<GetInventoryItemBannerResponse>>(result));
        }
    }
}
