using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.FavouriteListItems.Commands;
using NettoRetail.Application.CQRS.FavouriteListItems.Queries;
using NettoRetail.Application.Models.Request.FavouriteListItem;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.API.Controllers.V1
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class FavouriteListItemController(IMediator mediator) : ControllerBase
    {
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AddToFavouritesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AddToFavouritesResponse>>> AddToFavourite(AddToFavouriteRequest request, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            var command = new AddToFavouriteListCommand(userId, request.InventoryItemId);
            var result = await mediator.Send(command, cancellationToken);
            var response = new ApiResponse<AddToFavouritesResponse>(result);
            return Ok(response);
        }

        [Authorize]
        [HttpDelete("{itemId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveFromFavourites(Guid itemId, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();
            var command = new RemoveFromFavouriteListCommand(userId, itemId);
            await mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<ApiResponse<GetFavouriteResponse>>> GetFavorites(CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetUserId();     // Guid
            var shopId = HttpContext.GetUserShopId(); // int

            var query = new GetFavouriteListQuery(userId, shopId);
            var vm = await mediator.Send(query, cancellationToken);

            return Ok(new ApiResponse<GetFavouriteResponse>(vm));
        }

        [HttpGet("resolve")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<GetFavouriteResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<GetFavouriteResponse>>> ResolveFavourites(
        [FromQuery(Name = "ids")] List<Guid>? ids,
        CancellationToken cancellationToken)
        {
            var shopId = HttpContext.GetUserShopId(); 

            var vm = await mediator.Send(
                new ResolveGuestFavouritesQuery(ids ?? new List<Guid>(), shopId),
                cancellationToken
            );

            return Ok(new ApiResponse<GetFavouriteResponse>(vm));
        }
    }
}
