using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.InventoryItems.Commands;
using NettoRetail.Application.CQRS.InventoryItems.Queries;
using NettoRetail.Application.Models.InventoryItem;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/inventory-items")]
public class InventoryItemsController(IMediator mediator, IAuthorizationService authorizationService) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddInventoryItem(AddInventoryItemModel model)
    {
        var inventoryItemCreationCommand = new CreateInventoryItemCommand(model);

        var data = await mediator.Send(inventoryItemCreationCommand);

        var response = new ApiResponse<InventoryItemResponse>(data);

        return Created(string.Empty, response);
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<IActionResult> EditInventoryItemGeneralInfo(Guid id, UpdateInventoryItemGeneralInfoModel model, CancellationToken cancellationToken)
    {
        var inventoryItemCreationCommand = new UpdateInventoryItemGeneralInfoCommand(id, model);

        await mediator.Send(inventoryItemCreationCommand, cancellationToken);

        return NoContent();
    }

    [Authorize] //(Policy = PolicyNames.HasShopClaimPolicy)
    [HttpGet]
    public async Task<IActionResult> GetFilteredInventoryItems([FromBody] GetFilteredInventoryItemsModel model, [FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        var authResult = await authorizationService.AuthorizeAsync(User, PolicyNames.HasShopClaimPolicy);
        if (!authResult.Succeeded)
            return Unauthorized();

        if (!int.TryParse(HttpContext.User.FindFirst("shop_id")?.Value, out int userShopId))
        {
            return BadRequest("Couldn't retrieve shop id from claims");
        }

        var getFilteredInventoryItemsQuery = new GetFilteredInventoryItemsQuery(model, userShopId, pageNumber, pageSize);

        var data = await mediator.Send(getFilteredInventoryItemsQuery);

        return Ok(data);
    }
}