using Asp.Versioning;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Application.CQRS.Roles.Queries;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.API.Controllers.V1;

[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
[ApiController]
public class RolesController(IRoleRepository roleRepository, IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddRole(CreateRoleRequest model, CancellationToken cancellationToken)
    {
        var command = model.Adapt<CreateRoleCommand>();

        var shopId = HttpContext.GetUserShopId();

        command = command with { ShopId = shopId };

        var role = await mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<CreateRoleResponse>(role));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var query = new GetAllRolesQuery(shopId);
        var data = await mediator.Send(query, cancellationToken);

        var response = new ApiResponse<IEnumerable<GetAllRolesResponse>>(data);

        return Ok(response);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRole(int id, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var command = new DeleteRoleCommand(id, shopId);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
}
