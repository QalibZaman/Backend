using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.Application.CQRS.NewProductsSection.Command;
using NettoRetail.Application.CQRS.NewProductsSection.Queries;
using NettoRetail.Application.Models.NewProductsSection;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class NewProductsSectionController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateNewProductsSection(
        int shopId,
        [FromBody] UpdateNewProductsSectionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateNewProductsSectionCommand(
            shopId,
            request);

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
