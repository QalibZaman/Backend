using Asp.Versioning;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<BrandResponse>>), StatusCodes.Status200OK)]
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
    [HttpGet("{brandId:int}")]
    [ProducesResponseType(typeof(ApiResponse<BrandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> GetBrand([FromQuery] int brandId, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var getBrandInfoQuery = new GetBrandInfoQuery(brandId, shopId);
        var data = await mediator.Send(getBrandInfoQuery, cancellationToken);

        var response = new ApiResponse<BrandResponse>(data);

        return Ok(response);
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
    public async Task<IActionResult> UpdateBrand(
        int brandId,
        [FromForm] UpdateBrandRequest model,
        CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var command = model.Adapt<UpdateBrandCommand>();
        command = command with { Id = brandId, ShopId = shopId };
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
}
