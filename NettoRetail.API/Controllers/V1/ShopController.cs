using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.Shops.Commands;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Authorize]
[Route("/api/v{version:apiVersion}/[controller]")]
public class ShopController(IMediator mediator) : ControllerBase
{
    [HttpDelete("delete-logo")]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    public async Task<IActionResult> DeleteLogo()
    {
        var shopId = HttpContext.GetUserShopId();
        var command = new DeleteLogoCommand(shopId);
        await mediator.Send(command);
        return Ok();
    }
}