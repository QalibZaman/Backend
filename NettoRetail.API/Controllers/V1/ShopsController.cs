using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Attributes;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Shops.Commands;
using NettoRetail.Application.CQRS.Shops.Queries;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Shop;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class ShopsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetShop(int id)
    {
        var getShopByIdQuery = new GetShopByIdQuery(id);
        var data = await mediator.Send(getShopByIdQuery);

        var response = new ApiResponse<ShopResponse>(data);

        return Ok(response);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateShop([FromBody] ShopModel shop)
    {
        var createShopCommand = new CreateShopCommand(HttpContext.GetUserId(), shop);
        var data = await mediator.Send(createShopCommand);

        var response = new ApiResponse<ShopResponse>(data);

        return CreatedAtAction(nameof(GetShop), new { id = data.Id }, response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPut]
    public async Task<IActionResult> UpdateShop([FromBody] ShopModel shop)
    {
        var shopId = HttpContext.GetUserShopId();
        var updateShopCommand = new UpdateShopCommand(shopId, shop);
        await mediator.Send(updateShopCommand);

        return NoContent();
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPut("activation/{shopId:int}")]
    public async Task<IActionResult> ActivateShop(int shopId)
    {
        var activateShopCommand = new ActivateShopCommand(shopId);

        await mediator.Send(activateShopCommand);

        return Ok("Shop successfully activated.");
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPut("deactivation/{shopId:int}")]
    public async Task<IActionResult> DeactivateShop(int shopId, [FromBody] DeactivateShopRequest request)
    {
        var deactivateShopCommand = new DeactivateShopCommand(shopId, request.Reason, request.DeactivationDate);

        await mediator.Send(deactivateShopCommand);

        return Ok("Shop successfully deactivated.");
    }
}
