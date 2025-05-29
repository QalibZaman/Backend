using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Contents.Commands;
using NettoRetail.Application.CQRS.Contents.Queries;
using NettoRetail.Application.Models.Request.Content;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{v:ApiVersion}/[controller]")]
public class ContentController(IMediator mediator) : Controller
{
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("{shopId:int}")]
    public async Task<IActionResult> GetAllContentBlocks([FromRoute] int shopId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllContentBlocksQuery(shopId), cancellationToken);
        var response = new ApiResponse<List<ContentBlockResponse>>(result);
        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("block/{id}")]
    public async Task<IActionResult> GetContentBlockById(int id, [FromQuery] int shopId)
    {
        var query = new GetContentBlockByIdQuery(id, shopId);
        var result = await mediator.Send(query);

        var response = new ApiResponse<ContentBlockResponse>(result);
        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPut("block/{slug}")] // Slug format: shopId-ContentCategory; Format: 1-
    public async Task<IActionResult> UpdateContentBySlug([FromQuery] string slug, [FromBody] UpdateContentRequest updateContentRequest)
    {
        var author = HttpContext.GetUserId();
        var command = new UpdateContentBySlugCommand(slug, author, updateContentRequest);
        await mediator.Send(command);

        return NoContent();
    }
}
