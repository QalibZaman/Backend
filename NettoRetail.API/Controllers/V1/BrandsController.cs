using Asp.Versioning;
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
    [HttpPost]
    public async Task<IActionResult> CreateBrand([FromForm] CreateBrandRequest model, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var createBrandCommand = new CreateBrandCommand(model, shopId);
        var data = await mediator.Send(createBrandCommand, cancellationToken);

        var response = new ApiResponse<BrandResponse>(data);

        // TODO: Change to createdat Get once we add an appropriate endpoint
        return Created(string.Empty, response);
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
}
