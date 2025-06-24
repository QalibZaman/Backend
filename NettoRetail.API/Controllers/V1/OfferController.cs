using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.Application.CQRS.OfferSections.Command;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class OfferController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateOffersSection(
        int shopId,
        [FromBody] UpdateOffersSectionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOffersSectionCommand(
            shopId,
            request.Name.Trim(),
            request.Preview?.Trim(),
            request.IsVisible,
            request.SelectionType,
            request.InventoryItemIds,
            request.UpdatedBy);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
}
