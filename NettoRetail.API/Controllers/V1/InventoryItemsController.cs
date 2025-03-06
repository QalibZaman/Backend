using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.Application.CQRS.Users.Commands;
using NettoRetail.Application.Models.InventoryItem;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class InventoryItemsController(IMediator mediator) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> Create(InventoryItemModel model)
    {
        var inventoryItemCreationCommand = new CreateInventoryItemCommand(model);

        await mediator.Send(inventoryItemCreationCommand);

        return NoContent();
    }
}