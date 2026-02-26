using Asp.Versioning;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Attributes;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Brands.Commands;
using NettoRetail.Application.CQRS.Brands.Queries;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Brand;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Brand;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class BrandsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns the brands list for the current shop (paginated).
    /// </summary>
    /// <remarks>
    /// US-9.1 View Brands List.
    /// <para>Request: <c>GET /api/v1/brands?PageNumber=1&amp;PageSize=20</c></para>
    /// <para>Auth: Bearer token with shop claim is required.</para>
    /// <para>Success response (200):</para>
    /// <para>
    /// <code>
    /// {
    ///   "data": {
    ///     "items": [
    ///       {
    ///         "id": 5,
    ///         "name": "Acme",
    ///         "description": "Brand description",
    ///         "websiteUrl": "https://acme.example",
    ///         "logoImageUrl": "https://cdn.example/brand/acme.png"
    ///       }
    ///     ],
    ///     "totalAmount": 42
    ///   }
    /// }
    /// </code>
    /// </para>
    /// <para>Notes:</para>
    /// <para>- Pagination is 1-based: <c>PageNumber</c> starts from 1.</para>
    /// <para>- <c>logoImageUrl</c> can be null.</para>
    /// <para>Error response shape:</para>
    /// <para>
    /// <code>
    /// { "message": "Error message", "errors": { "field": [ "details" ] } }
    /// </code>
    /// </para>
    /// </remarks>
    /// <param name="pagination">Pagination parameters: <c>PageNumber</c> and <c>PageSize</c>.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>200 with a paginated list of brands.</returns>
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [RequiresAnyPermission]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<BrandResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> GetShopBrands([FromQuery] PaginationRequest pagination ,CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var query = new GetShopBrandsQuery(shopId, pagination);
        var data = await mediator.Send(query, cancellationToken);

        var response = new ApiResponse<PaginatedResponse<BrandResponse>>(data);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [RequiresAnyPermission]
    [HttpGet("{brandId:int}")]
    [ProducesResponseType(typeof(ApiResponse<BrandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> GetBrand(
        [FromRoute] int brandId,
        CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var data = await mediator.Send(
            new GetBrandInfoQuery(brandId, shopId),
            cancellationToken);

        return Ok(new ApiResponse<BrandResponse>(data));
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BrandResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json", "multipart/form-data")]
    public async Task<IActionResult> CreateBrand([FromForm] CreateBrandRequest model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var createBrandCommand = new CreateBrandCommand(model, shopId);
        var data = await mediator.Send(createBrandCommand, cancellationToken);

        var response = new ApiResponse<BrandResponse>(data);

        return CreatedAtAction(nameof(GetBrand), new { brandId = data.Id }, response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpDelete("{brandId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteBrand(int brandId, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var deleteBrandCommand = new DeleteBrandCommand(brandId, shopId);
        await mediator.Send(deleteBrandCommand, cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPut("{brandId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json", "multipart/form-data")]
    public async Task<IActionResult> UpdateBrand(int brandId, [FromForm] UpdateBrandRequest model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var command = new UpdateBrandCommand(brandId, model, shopId);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
}
