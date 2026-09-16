using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.Shops.Commands;
using NettoRetail.Application.Models.Request.Shop;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Authorize]
[Route("/api/v{version:apiVersion}/[controller]")]
public class ShopController(IMediator mediator) : ControllerBase
{
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

    [HttpPatch("logo")]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeLogo(AddLogoRequest request)
    {
        var shopId = HttpContext.GetUserShopId();
        var command = new ChangeLogoCommand(request, shopId);
        await mediator.Send(command);
        return Ok();
    }
}