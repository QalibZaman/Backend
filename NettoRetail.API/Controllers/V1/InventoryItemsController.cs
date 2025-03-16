using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.InventoryItems.Commands;
using NettoRetail.Application.CQRS.InventoryItems.Queries;
using NettoRetail.Application.Models.InventoryItem;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/inventory-items")]
public class InventoryItemsController(IMediator mediator) : ControllerBase
{
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
}