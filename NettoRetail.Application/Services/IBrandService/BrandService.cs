using Mapster;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NettoRetail.Application.Models.Response.Brand;

namespace NettoRetail.Application.Services.IBrandService;

public class BrandService(IBrandRepository brandRepository) : IBrandService
{
    public async Task<PaginatedResponse<BrandResponse>> GetShopBrandsBySearchAsync(
         string query, int shopId, PaginationRequest pageModel, CancellationToken cancellationToken)
    {
        var skip = (pageModel.PageNumber - 1) * pageModel.PageSize;
        var brandSpec = new SpecificationBuilder<BrandEntity>()
            .Add<GetBrandsByPartialNameSpecification>(query)
            .Add<GetBrandsByShopIdSpecification>(shopId)
            .AddOrderBy(b => b.Name)
            .AddOrderBy(b => b.Id)
            .Skip(skip)
            .Take(pageModel.PageSize)
            .Build();

        var (items, totalCount) = await brandRepository.GetAllPaginatedAsync(brandSpec, cancellationToken);

        var brandResponses = items.Adapt<List<BrandResponse>>();

        var response = new PaginatedResponse<BrandResponse>
        {
            Items = brandResponses,
            TotalAmount = totalCount,
        };
        return response;
    }

    public async Task<BrandEntity> GetBrandByNameWithCacheAsync(
      Dictionary<string, BrandEntity> cache,
      string brandName,
      int shopId,
      CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(brandName, out var cached))
            return cached;

        var specBuilder = new SpecificationBuilder<BrandEntity>();
        specBuilder
            .Add<GetBrandsByNameSpecification>(brandName)
            .Add<GetBrandsByShopIdSpecification>(shopId);

        var spec = specBuilder.Build();
        var brand = await brandRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException($"Brand '{brandName}' not found.");

        cache[brandName] = brand;
        return brand;
    }
}