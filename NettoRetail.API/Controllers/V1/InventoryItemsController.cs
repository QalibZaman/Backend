using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.InventoryItems.Commands;
using NettoRetail.Application.CQRS.InventoryItems.Queries;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/inventory-items")]
public class InventoryItemsController(IMediator mediator, IAuthorizationService authorizationService) : ControllerBase
{
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetInventoryItemGeneralInfo(Guid id, CancellationToken cancellationToken)
    {
        var inventoryItemQuery = new GetInventoryItemGeneralInfoQuery(id);
        var data = await mediator.Send(inventoryItemQuery, cancellationToken);
        var response = new ApiResponse<InventoryItemGeneralInfoResponse>(data);

        return Ok(response);
    }

    [Authorize]
    [HttpGet("csv-file")]
    public async Task<IActionResult> DownloadInventoryItemsList(CancellationToken cancellationToken)
    {
        var inventoryItemsListDownloadQuery = new DownloadInventoryItemsListQuery();

        var response = await mediator.Send(inventoryItemsListDownloadQuery, cancellationToken);

        return File(response, "text/csv", "Inventory items.csv");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddInventoryItem(AddInventoryItemModel model)
    {
        var authResult = await authorizationService.AuthorizeAsync(User, PolicyNames.HasShopClaimPolicy);
        if (!authResult.Succeeded)
            return Unauthorized();

        var userShopId = HttpContext.GetUserShopId();

        var inventoryItemCreationCommand = new CreateInventoryItemCommand(model, userShopId);

        var data = await mediator.Send(inventoryItemCreationCommand);

        var response = new ApiResponse<InventoryItemResponse>(data);

        return Created(string.Empty, response);
    }

    [Authorize]
    [HttpPatch("{id}")]
    public async Task<IActionResult> EditInventoryItemGeneralInfo(Guid id, UpdateInventoryItemGeneralInfoModel model, CancellationToken cancellationToken)
    {
        var inventoryItemUpdatingCommand = new UpdateInventoryItemGeneralInfoCommand(id, model);

        await mediator.Send(inventoryItemUpdatingCommand, cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetFilteredInventoryItems([FromQuery] GetFilteredInventoryItemsModel model, [FromQuery] int pageNumber, [FromQuery] int pageSize, CancellationToken cancellationToken)
    {
        var authResult = await authorizationService.AuthorizeAsync(User, PolicyNames.HasShopClaimPolicy);
        if (!authResult.Succeeded)
            return Unauthorized();

        var userShopId = HttpContext.GetUserShopId();

        var getFilteredInventoryItemsQuery = new GetFilteredInventoryItemsQuery(model, userShopId, pageNumber, pageSize);

        var data = await mediator.Send(getFilteredInventoryItemsQuery, cancellationToken);

        return Ok(data);
    }

    [Authorize]
    [HttpGet("search")]
    public async Task<IActionResult> GetInventoryItemByUpcOrName([FromQuery] GetInventoryItemsByUpcOrNameModel model, CancellationToken cancellationToken)
    {
        var authResult = await authorizationService.AuthorizeAsync(User, PolicyNames.HasShopClaimPolicy);
        if (!authResult.Succeeded)
            return Unauthorized();

        var userShopId = HttpContext.GetUserShopId();

        var getFilteredInventoryItemsQuery = new GetInventoryItemsByUpcOrNameQuery(model, userShopId);

        var data = await mediator.Send(getFilteredInventoryItemsQuery, cancellationToken);

        return Ok(data);
    }
}