using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Comparison.Commands;
using NettoRetail.Application.CQRS.Comparison.Queries;
using NettoRetail.Application.Models.Request.Comparison;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Comparison;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{v:ApiVersion}/[controller]")]
public class ComparisonController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ComparisonItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ComparisonItemResponse>>> Get(
        [FromQuery] int categoryId,
        CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        if (userId == Guid.Empty || categoryId == 0 || categoryId < 0)
            return BadRequest("userId and categoryId are required.");

        var data = await mediator.Send(new GetComparisonListQuery(userId, categoryId), ct);
        return Ok(data);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ComparisonItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ComparisonItemResponse>> AddItem(
    [FromQuery] AddComparisonItemRequest request,
    CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();

        if (userId == Guid.Empty || request.InventoryItemId == Guid.Empty)
            return BadRequest("Invalid userId or inventoryItemId");

        var result = await mediator.Send(
            new AddItemToComparisonCommand(
            userId,
            request.InventoryItemId),
            ct);

        return Ok(result);
    }

    [Authorize]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveFromComparison(
        [FromQuery] RemoveComparisonItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        if (userId == Guid.Empty || request.InventoryItemId == Guid.Empty)
            return BadRequest("Invalid userId or inventoryItemId");

        var command = new RemoveItemFromComparisonCommand(userId, request.InventoryItemId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}