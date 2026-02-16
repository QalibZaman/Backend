using Mapster;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
﻿using NettoRetail.Application.Exceptions;
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