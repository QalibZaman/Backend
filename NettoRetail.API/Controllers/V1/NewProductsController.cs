using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.NewProductsSection.Commands;
using NettoRetail.Application.CQRS.NewProductsSection.Queries;
using NettoRetail.Application.Models.NewProductsSection;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class NewProductsSectionController(IMediator mediator) : ControllerBase
{
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPut]
    public async Task<IActionResult> UpdateNewProductsSection(
        [FromBody] UpdateNewProductsSectionRequest request, CancellationToken cancellationToken)
    {
        var userShopId = HttpContext.GetUserShopId();

        var command = new UpdateNewProductsSectionCommand(userShopId, request);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet("{shopId}")]
    public async Task<IActionResult> GetNewProductsSection(
          int shopId,
          CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNewProductsSectionByShopIdQuery(shopId), cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }
}
