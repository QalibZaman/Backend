using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Attributes;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.InventoryItems.Commands;
using NettoRetail.Application.CQRS.InventoryItems.Queries;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Request.InventoryItem;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/inventory-items")]
public class InventoryItemsController(IMediator mediator) : ControllerBase
{
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemInfoFullResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id:guid}/full-info")]
    public async Task<IActionResult> GetInventoryItemFullInfo(Guid id, CancellationToken cancellationToken)
    {
        var inventoryItemQuery = new GetInventoryItemFullInfoQuery(id);
        var data = await mediator.Send(inventoryItemQuery, cancellationToken);
        var response = new ApiResponse<InventoryItemInfoFullResponse>(data);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id:guid}/details")]
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

    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> AddInventoryItem(AddInventoryItemModel model)
    {
        var userShopId = HttpContext.GetUserShopId();

        var inventoryItemCreationCommand = new CreateInventoryItemCommand(model, userShopId);

        var data = await mediator.Send(inventoryItemCreationCommand);

        var response = new ApiResponse<InventoryItemResponse>(data);

        return Created(string.Empty, response);
    }

    /// <summary>
    ///  Uploads the list of items from the '.csv' file.
    /// </summary>
    /// <remarks>
    ///     The '.csv' file should be in correct format, example of the file content:
    ///     <code>
    ///         Product UPC,Product Name,Product Location,Brand,Product Category,Available Amount,Reserved Amount,On-hand Amount,Price,Status,Description
    ///         123456789012,Wireless Mouse,"Warehouse 3, Aisle 4",Gembird,ElectronicsGadgets,70,10,90,25.99,Available,Ergonomic wireless mouse with adjustable DPI.
    ///     </code>
    ///     **Notice these rules:**
    ///     1. No space after a comma char.
    ///     2. Values that need comma are enclosed in double quotes.
    ///     3. Brand and category must exist in the database.
    /// </remarks>
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost("csv-file")]
    public async Task<IActionResult> UploadInventoryItemsList(IFormFile itemsCsvFile, CancellationToken cancellationToken)
    {
        var userShopId = HttpContext.GetUserShopId();

        var inventoryItemsListUploadCommand = new UploadInventoryItemsListCommand(itemsCsvFile, userShopId);

        await mediator.Send(inventoryItemsListUploadCommand, cancellationToken);

        return Ok();
    }

    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPatch("full-info/{id:guid}")]
    public async Task<IActionResult> EditInventoryItem(Guid id, [FromBody] UpdateProductModel model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        await mediator.Send(new UpdateProductCommand(id, shopId, model), cancellationToken);

        return NoContent();
    }

    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> EditInventoryItem(Guid id, [FromBody] UpdateInventoryItemModel model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        await mediator.Send(new UpdateInventoryItemCommand(id, shopId, model), cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPatch("{itemId}/details")]
    public async Task<IActionResult> EditInventoryItemDetails(Guid itemId, [FromBody] UpsertInventoryItemDetailsModel model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var inventoryItemUpdatingCommand = new UpsertInventoryItemDetailsCommand(itemId, shopId, model);

        await mediator.Send(inventoryItemUpdatingCommand, cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("getInventoryItems")]
    public async Task<IActionResult> GetInventoryItems([FromQuery] InventoryItemsSearchModel model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var result = await mediator.Send(new GetInventoryItemsQuery(shopId, model), cancellationToken);
        return Ok(new ApiResponse<PaginatedResponse<InventoryItemListResponse>>(result));
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [ProducesResponseType(typeof(ApiResponse<ProductDescriptionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}/description")]
    public async Task<IActionResult> GetInventoryItemDescription(Guid id, CancellationToken cancellationToken)
    {
        var descriptionText = await mediator.Send(new GetInventoryItemDescriptionQuery(id), cancellationToken);
        var response = new ApiResponse<ProductDescriptionResponse>(new ProductDescriptionResponse { Description = descriptionText });
        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("{id}/description")]
    public async Task<IActionResult> UpdateInventoryItemDescription(Guid id, [FromBody] UpdateProductsDescriptionRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateInventoryItemDescriptionCommand(id, request.Description), cancellationToken);
        return Ok();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [ProducesResponseType(typeof(ApiResponse<UploadedImageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("{id}/description/image")]
    public async Task<IActionResult> UploadDescriptionImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        var imageUrl = await mediator.Send(new UploadDescriptionImageCommand(file), cancellationToken);
        var response = new ApiResponse<UploadedImageResponse>(
            new UploadedImageResponse { FileUrl = imageUrl });
        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost("{id}/images")]
    public async Task<IActionResult> UploadInventoryItemImage(Guid id, IFormFile image, CancellationToken cancellationToken)
    {
        var command = new UploadInventoryItemsImages(id, image);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ItemComparisonResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("comparison/{itemId}")]
    public async Task<IActionResult> AddToComparison(Guid itemId, CancellationToken cancellationToken)
    {
        Guid userId = HttpContext.GetUserId();

        var command = new AddItemToComparisonCommand(itemId, userId);

        var response = await mediator.Send(command, cancellationToken);

        return Ok(response);
    }

    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ItemComparisonResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("comparison/{itemId}")]
    public async Task<IActionResult> DeleteFromComparison(Guid itemId, CancellationToken cancellationToken)
    {
        Guid userId = HttpContext.GetUserId();

        var command = new DeleteItemFromComparisonCommand(itemId, userId);

        var response = await mediator.Send(command, cancellationToken);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("parent/toggleLink")]
    public async Task<IActionResult> ToggleLinkProducts([FromBody] ToggleLinkProductsRequest request, CancellationToken cancellationToken)
    {
        var data = await mediator.Send(new ToggleLinkProductsCommand(request.ParentId, request.ChildId), cancellationToken);
        return Ok(data);
    }
}