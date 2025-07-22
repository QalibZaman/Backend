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
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ShopResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetShop(int id)
    {
        var getShopByIdQuery = new GetShopByIdQuery(id);
        var data = await mediator.Send(getShopByIdQuery);

        var response = new ApiResponse<ShopResponse>(data);

        return Ok(response);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ShopResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ShopResponse>>> CreateShop([FromBody] ShopModel shop)
    {
        var userId = HttpContext.GetUserId();
        var command = new CreateShopCommand(userId, shop);
        var createdShop = await mediator.Send(command);

        var response = new ApiResponse<ShopResponse>(createdShop);

        return CreatedAtAction(nameof(GetShop), new { id = createdShop.Id }, response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateShop([FromBody] ShopModel shop)
    {
        var shopId = HttpContext.GetUserShopId();

        var command = new UpdateShopCommand(shopId, shop);
        await mediator.Send(command);

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
        var command = new ActivateShopCommand(shopId);

        await mediator.Send(command);

        return Ok("Shop successfully activated.");
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize]
    [HttpPut("deactivation/{shopId:int}")]
    public async Task<IActionResult> DeactivateShop(int shopId, [FromBody] DeactivateShopRequest request)
    {
        var command = new DeactivateShopCommand(shopId, request.Reason, request.DeactivationDate);

        await mediator.Send(command);

        return Ok("Shop successfully deactivated.");
    }
}
