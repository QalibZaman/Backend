using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.TeaserSection.Commands;
using NettoRetail.Application.CQRS.TeaserSection.Queries;
using NettoRetail.Application.Requests.TeaserSection;
using NettoRetail.Application.Responses.TeaserSection;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/teaser-section")]
public class TeaserSectionController(IMediator mediator) : ControllerBase
{
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<TeaserSectionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> GetTeaserSection(CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var query = new GetTeaserSectionQuery(shopId);
        var data = await mediator.Send(query, cancellationToken);

        if (data is null)
            return NotFound();

        var response = new ApiResponse<TeaserSectionResponse>(data);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<TeaserSectionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateTeaserSection([FromBody] TeaserSectionRequest model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var updatedBy = User?.Identity?.Name ?? "system";

        var command = new UpdateTeaserSectionCommand(model, shopId, updatedBy);
        var data = await mediator.Send(command, cancellationToken);

        var response = new ApiResponse<TeaserSectionResponse>(data);

        return Ok(response);
    }
}
