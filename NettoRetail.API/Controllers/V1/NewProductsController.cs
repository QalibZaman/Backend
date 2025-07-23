using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.Application.CQRS.NewProducts.Command;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class NewProductsController(IMediator mediator) : ControllerBase
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
}
