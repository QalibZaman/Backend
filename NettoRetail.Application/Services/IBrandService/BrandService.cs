using Mapster;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.IBrandService;

public class BrandService(IBrandRepository brandRepository) : IBrandService
{
    public async Task<PaginatedResponse<BrandResponse>> GetShopBrandsBySearchAsync(
         string query, int shopId, PaginationRequest pageModel, CancellationToken cancellationToken)
    {

        var brandSpecBuilder = new SpecificationBuilder<BrandEntity>();
        brandSpecBuilder
            .Add<GetBrandsByPartialNameSpecification>(query)
            .Add<GetBrandsByShopIdSpecification>(shopId);

        var brandSpec = brandSpecBuilder.Build();

        var brands = await brandRepository.GetAllBySpecAsync(brandSpec, cancellationToken);

        var pagedBrands = brands
            .Skip((pageModel.PageNumber - 1) * pageModel.PageSize)
            .Take(pageModel.PageSize)
            .ToList();

        var brandResponses = pagedBrands.Adapt<List<BrandResponse>>();

        var response = new PaginatedResponse<BrandResponse>
        {
            Items = brandResponses,
            TotalAmount = brands.Count(),
        };
        return response;
    }
}