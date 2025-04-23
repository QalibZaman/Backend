using Asp.Versioning;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Brands.Commands;
using NettoRetail.Application.CQRS.Brands.Queries;
using NettoRetail.Application.Models.Request.Brand;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:apiVersion}/[controller]")]
public class BrandsController(IMediator mediator) : ControllerBase
{
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet]
    public async Task<IActionResult> GetShopBrands(CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();
        var getShopBrandsQuery = new GetShopBrandsQuery(shopId);
        var data = await mediator.Send(getShopBrandsQuery, cancellationToken);

        var response = new ApiResponse<IEnumerable<BrandResponse>>(data);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("{brandId:int}")]
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
    public async Task<IActionResult> CreateBrand([FromForm] CreateBrandRequest model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var createBrandCommand = new CreateBrandCommand(model, shopId);
        var data = await mediator.Send(createBrandCommand, cancellationToken);

        var response = new ApiResponse<BrandResponse>(data);

        return CreatedAtAction(nameof(GetBrand), new { id = data.Id }, response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpDelete("{brandId:int}")]
    public async Task<IActionResult> DeleteBrand(int brandId, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var deleteBrandCommand = new DeleteBrandCommand(brandId, shopId);
        await mediator.Send(deleteBrandCommand, cancellationToken);

        return Ok();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPut("{brandId:int}")]
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
