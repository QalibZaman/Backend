using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.CQRS.Shops.Commands;
using NettoRetail.Application.CQRS.Shops.Queries;
using NettoRetail.Application.Models.Shop;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class ShopsController(IMediator mediator, IAuthorizationService authorizationService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetShop(int id)
    {
        var getShopByIdQuery = new GetShopByIdQuery(id);
        var response = await mediator.Send(getShopByIdQuery);

        return Ok(response);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateShop([FromBody] ShopModel shop)
    {
        var createShopCommand = new CreateShopCommand(HttpContext.GetUserId(), shop);
        var response = await mediator.Send(createShopCommand);

        return Ok(response);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateShop(int id, [FromBody] ShopModel shop)
    {
        var authResult = await authorizationService.AuthorizeAsync(User, id, PolicyNames.ShopPolicy);
        if (!authResult.Succeeded)
            return Unauthorized();

        var updateShopCommand = new UpdateShopCommand(id, shop);
        await mediator.Send(updateShopCommand);

        return Ok();
    }
}
