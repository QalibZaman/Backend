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
    /// <summary>
    /// Returns all content blocks for the specified shop (CMS overview screen).
    /// </summary>
    /// <remarks>
    /// US-3.10 View content management.
    /// <para>Request: <c>GET /api/v1/content/{shopId}</c></para>
    /// <para>Auth: Bearer token with shop claim is required.</para>
    /// <para>Response: list of content blocks for the shop.</para>
    /// </remarks>
    /// <param name="shopId">Shop identifier from the route.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>200 with a list of content blocks.</returns>
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("{shopId:int}")]
    [ProducesResponseType(typeof(ApiResponse<List<ContentBlockResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<ContentBlockResponse>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<List<ContentBlockResponse>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<List<ContentBlockResponse>>), StatusCodes.Status500InternalServerError)]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetAllContentBlocks([FromRoute] int shopId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllContentBlocksQuery(shopId), cancellationToken);
        var response = new ApiResponse<List<ContentBlockResponse>>(result);
        return Ok(response);
    }

    /// <summary>
    /// Returns a single content block by id for the given shop.
    /// </summary>
    /// <remarks>
    /// US-3.10 View content management.
    /// <para>Request: <c>GET /api/v1/content/block/{id}?shopId=123</c></para>
    /// <para>Auth: Bearer token with shop claim is required.</para>
    /// <para>Response: one content block.</para>
    /// </remarks>
    /// <param name="id">Content block identifier from the route.</param>
    /// <param name="shopId">Shop identifier from the query string.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>200 with a single content block.</returns>
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("block/{id}")]
    [ProducesResponseType(typeof(ApiResponse<ContentBlockResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ContentBlockResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ContentBlockResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ContentBlockResponse>), StatusCodes.Status500InternalServerError)]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetContentBlockById(int id, [FromQuery] int shopId, CancellationToken cancellationToken)
    {
        var query = new GetContentBlockByIdQuery(id, shopId);
        var result = await mediator.Send(query, cancellationToken);

        var response = new ApiResponse<ContentBlockResponse>(result);
        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPut("block/{slug}")] // Slug format: shopId-ContentCategory; Example: 1-OurShop
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> UpdateContentBySlug(string slug, [FromBody] UpdateContentRequest updateContentRequest)
    {
        var author = HttpContext.GetUserId();
        var command = new UpdateContentBySlugCommand(slug, author, updateContentRequest);
        await mediator.Send(command);

        return NoContent();
    }
}
