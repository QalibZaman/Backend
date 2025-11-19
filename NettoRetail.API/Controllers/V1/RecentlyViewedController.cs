using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.RecentlyViewedProducts.Commands;
using NettoRetail.Application.CQRS.RecentlyViewedProducts.Queries;
using NettoRetail.Application.Models.Request.RecentlyViewedProduct;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.API.Controllers.V1;
[ApiController]
[ApiVersion(1)]
[Route("api/v{v:ApiVersion}/[controller]")]
[Authorize]
public class RecentlyViewedController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("{inventoryItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Add(Guid inventoryItemId, CancellationToken cancellationToken)
    {
        var accountId = HttpContext.GetUserId();

        await _mediator.Send(new AddRecentlyViewedProductCommand(accountId, inventoryItemId), cancellationToken);

        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<InventoryItemCardResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        var accountId = HttpContext.GetUserId();

        var result = await _mediator.Send(new GetRecentlyViewedProductsQuery(accountId), cancellationToken);

        return Ok(new ApiResponse<List<InventoryItemCardResponse>>(result));
    }

    [HttpPost("merge")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Merge([FromBody] MergeRecentlyViewedRequest request, CancellationToken cancellationToken)
    {
        var accountId = HttpContext.GetUserId();

        await _mediator.Send(new MergeRecentlyViewedProductsCommand(accountId, request.Items), cancellationToken);

        return NoContent();
    }
}
