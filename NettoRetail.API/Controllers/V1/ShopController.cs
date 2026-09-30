using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.Shops.Commands;
using NettoRetail.Application.Models.Link;
using NettoRetail.Application.Models.Request.Shop;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Authorize]
[Route("/api/v{version:apiVersion}/[controller]")]
public class ShopController(IMediator mediator) : ControllerBase
{
    [HttpDelete("logo")]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    public async Task<IActionResult> DeleteLogo()
    {
        var shopId = HttpContext.GetUserShopId();
        var command = new DeleteLogoCommand(shopId);
        await mediator.Send(command);
        return Ok();
    }

    [HttpPost("logo")]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddLogo(AddLogoRequest request)
    {
        var shopId = HttpContext.GetUserShopId();
        var command = new AddLogoCommand(request, shopId);
        await mediator.Send(command);
        return Ok();
    }

    [HttpPost("social-links")]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddSocialLinks(
    AddSocialLinksRequest request)
    {
        var shopId = HttpContext.GetUserShopId();

        var command = new AddSocialLinksCommand(request, shopId);

        await mediator.Send(command);

        return NoContent();
    }
}