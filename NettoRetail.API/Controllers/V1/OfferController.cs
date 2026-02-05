using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.OfferSections.Command;
using NettoRetail.Application.CQRS.OfferSections.Queries;
using NettoRetail.Application.CQRS.OfferSections.Query;
using NettoRetail.Application.Models.Request.Offer;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class OfferController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOffersSection(
        int shopId,
        [FromBody] UpdateOffersSectionRequest request,
        CancellationToken cancellationToken)
    {
        var updatedBy = HttpContext.GetUpdatedByName();

        var command = new UpdateOffersSectionCommand(
            shopId,
            request.Name.Trim(),
            request.Preview?.Trim(),
            request.IsVisible,
            request.SelectionType,
            request.InventoryItemIds,
            updatedBy);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpGet("promotions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPromotionOffers(CancellationToken cancellationToken)
    {
        var promotionOffersQuery = new GetPromotionOffersQuery();
        var response = await mediator.Send(promotionOffersQuery, cancellationToken);
        return Ok(response);
    }

    [HttpGet("suggestions")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<SearchSuggestionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSearchSuggestions(
       [FromQuery] SearchSuggestionsRequest request,
       CancellationToken cancellationToken = default)
    {
        var suggestions = await mediator.Send(new GetSearchSuggestionsQuery(request), cancellationToken);
        var response = new ApiResponse<SearchSuggestionsResponse>(suggestions);
        return Ok(response);
    }
}
