using MapsterMapper;
using NettoRetail.Application.Models.Response.Brand;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.IBrandService;

public class BrandService(IShopRepository shopRepository, IBrandRepository brandRepository, IMapper mapper) : IBrandService
{
    public async Task<PaginatedResponse<BrandResponse>> GetShopBrandsBySearchAsync(
         string query, int shopId, PaginationRequest pageModel, CancellationToken cancellationToken)
    {
        var skip = PaginationHelper.CalculateSkip(pageModel.PageNumber, pageModel.PageSize);
        var brandSpec = new SpecificationBuilder<BrandEntity>()
            .Add<GetBrandsByPartialNameSpecification>(query)
            .Add<GetBrandsByShopIdSpecification>(shopId)
            .AddOrderBy(b => b.Name)
            .AddOrderBy(b => b.Id)
            .Skip(skip)
            .Take(pageModel.PageSize)
            .Build();

        var (items, totalCount) = await brandRepository.GetAllPaginatedAsync(brandSpec, cancellationToken);

        var brandResponses = mapper.Map<List<BrandResponse>>(items);

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

    public async Task EnsureBrandCanBeCreatedAsync(int shopId, string name, string? websiteUrl, CancellationToken cancellationToken)
    {
        var shopExists = await shopRepository.ExistsAsync(new GetShopByIdSpecification(shopId), cancellationToken);

        if (!shopExists)
            throw new BadRequestException($"Shop with id '{shopId}' does not exist.");

        if (await brandRepository.ExistsAsync(new GetBrandsByNameSpecification(name), cancellationToken))
            throw new BadRequestException("Brand's name must be unique");

        if (websiteUrl != null &&
            await brandRepository.ExistsAsync(new GetBrandsByWebsiteUrlSpecification(websiteUrl), cancellationToken))
            throw new BadRequestException("Brand's website url must be unique");
    }

}
