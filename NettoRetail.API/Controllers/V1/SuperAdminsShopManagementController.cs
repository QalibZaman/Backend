using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Attributes;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Shops.Queries;
using NettoRetail.Application.CQRS.SuperAdminsShopManagement.Commands;
using NettoRetail.Application.CQRS.SuperAdminsShopManagement.Queries;
using NettoRetail.Application.Models.ClientsShop;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/super-admins")]
public class SuperAdminsShopManagementController(IMediator mediator) : ControllerBase
{
    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateShopForClient(ClientsShopModel shop)
    {
        var createShopCommand = new CreateShopForClientCommand(shop);
        var data = await mediator.Send(createShopCommand);

        var response = new ApiResponse<ShopResponse>(data);

        return Created(string.Empty, response);
    }

    [RequiresPermission(Permissions.CUSTOMER_MANAGEMENT)]
    [RequiresPermission(Permissions.ORDERS_MANAGEMENT)]
    [RequiresPermission(Permissions.ADMINS_MANAGEMENT)]
    [RequiresPermission(Permissions.INVENTORY_MANAGEMENT)]
    [RequiresPermission(Permissions.SYSTEM_MANAGEMENT)]
    [RequiresPermission(Permissions.SUPERADMINS_MANAGEMENT)]
    [Authorize]
    [HttpGet("{shopId:int}")]
    public async Task<IActionResult> GetClientsShopInfo(int shopId)
    {
        var getClientsShopInfoQuery = new GetClientsShopInfoQuery(shopId);
        var data = await mediator.Send(getClientsShopInfoQuery);

        var response = new ApiResponse<ClientsShopInfoResponse>(data);

        return Ok(response);
    }
}