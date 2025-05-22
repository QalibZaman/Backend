using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.InventoryItems.Commands;
using NettoRetail.Application.CQRS.InventoryItems.Queries;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/inventory-items")]
public class InventoryItemsController(IMediator mediator) : ControllerBase
{
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetInventoryItemInfo(Guid id, CancellationToken cancellationToken)
    {
        var inventoryItemQuery = new GetInventoryItemInfoQuery(id);
        var data = await mediator.Send(inventoryItemQuery, cancellationToken);
        var response = new ApiResponse<InventoryItemInfoResponse>(data);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetInventoryItemDetails(Guid id, CancellationToken cancellationToken)
    {
        var inventoryDetailsQuery = new GetInventoryItemDetailsQuery(id);

        var data = await mediator.Send(inventoryDetailsQuery, cancellationToken);

        var response = new ApiResponse<IEnumerable<ProductDetailsResponse>>(data);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("csv-file")]
    public async Task<IActionResult> DownloadInventoryItemsList(CancellationToken cancellationToken)
    {
        var userShopId = HttpContext.GetUserShopId();

        var inventoryItemsListDownloadQuery = new DownloadInventoryItemsListQuery(userShopId);

        var response = await mediator.Send(inventoryItemsListDownloadQuery, cancellationToken);

        return File(response, "text/csv", "Inventory items.csv");
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost]
    public async Task<IActionResult> AddInventoryItem(AddInventoryItemModel model)
    {
        var userShopId = HttpContext.GetUserShopId();

        var inventoryItemCreationCommand = new CreateInventoryItemCommand(model, userShopId);

        var data = await mediator.Send(inventoryItemCreationCommand);

        var response = new ApiResponse<InventoryItemResponse>(data);

        return Created(string.Empty, response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost("csv-file")]
    public async Task<IActionResult> UploadInventoryItemsList(IFormFile itemsCsvFile, CancellationToken cancellationToken)
    {
        var userShopId = HttpContext.GetUserShopId();

        var inventoryItemsListUploadCommand = new UploadInventoryItemsListCommand(itemsCsvFile, userShopId);

        await mediator.Send(inventoryItemsListUploadCommand, cancellationToken);

        return Ok();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPatch("{id}")]
    public async Task<IActionResult> EditInventoryItem(Guid id, UpdateInventoryItemModel model, CancellationToken cancellationToken)
    {
        var inventoryItemUpdatingCommand = new UpdateInventoryItemCommand(id, model);

        await mediator.Send(inventoryItemUpdatingCommand, cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPatch("{id}/details")]
    public async Task<IActionResult> EditInventoryItemDetails(Guid id, [FromBody] UpdateInventoryItemDetailsModel model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var inventoryItemUpdatingCommand = new UpdateInventoryItemDetailsCommand(id, shopId, model);

        await mediator.Send(inventoryItemUpdatingCommand, cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet]
    public async Task<IActionResult> GetFilteredInventoryItems([FromQuery] GetFilteredInventoryItemsModel model, [FromQuery] PaginationRequest pageModel, CancellationToken cancellationToken)
    {
        var userShopId = HttpContext.GetUserShopId();

        var getFilteredInventoryItemsQuery = new GetFilteredInventoryItemsQuery(model, userShopId, pageModel);

        var data = await mediator.Send(getFilteredInventoryItemsQuery, cancellationToken);

        var response = new ApiResponse<PaginatedResponse<InventoryItemListResponse>>(data);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("search")]
    public async Task<IActionResult> GetInventoryItemByUpcOrName([FromQuery] GetInventoryItemsByUpcOrNameModel model, CancellationToken cancellationToken)
    {
        var userShopId = HttpContext.GetUserShopId();

        var getFilteredInventoryItemsQuery = new GetInventoryItemsByUpcOrNameQuery(model, userShopId);

        var data = await mediator.Send(getFilteredInventoryItemsQuery, cancellationToken);

        var response = new ApiResponse<IEnumerable<InventoryItemListResponse>>(data);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost("{id}/images")]
    public async Task<IActionResult> UploadInventoryItemImage(Guid id, IFormFile image, CancellationToken cancellationToken)
    {
        var command = new UploadInventoryItemsImages(id, image);
        await mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<string>("Image uploaded successfully"));
    }
}