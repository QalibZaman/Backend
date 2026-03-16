using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Variations.Commands;
using NettoRetail.Application.CQRS.Variations.Queries;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Application.Models.Response.InventoryItem.Variations;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class VariationsController(IMediator mediator) : ControllerBase
{
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateVariations([FromBody] CreateVariationsRequest model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var createVariationsCommand = new CreateVariationsCommand(model, shopId);
        await mediator.Send(createVariationsCommand, cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPatch("{id:guid}/toggle-active")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ToggleIsActiveStatus(Guid id, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var command = new ToggleIsActiveStatusCommand(id, shopId);

        var response = await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPatch("{id}/toggle-exists")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ToggleExists(string id, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        if (!Guid.TryParse(id, out var variationId))
            return BadRequest();

        var command = new ToggleExistsCommand(variationId, shopId);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("{parentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<VariationResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<VariationResponse>> GetItemVariations(Guid parentId, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var query = new GetItemVariationsQuery(parentId, shopId);

        var response = await mediator.Send(query, cancellationToken);

        return response;
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPut("variation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateVariation([FromForm] UpdateVariationRequest model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        await mediator.Send(new UpdateVariationCommand(model, shopId), cancellationToken);
        return NoContent();
    }
}
