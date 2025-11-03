using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.IBranService;

public class BrandService : IBrandService
{
    public async Task<BrandEntity> GetOrAddBrandAsync(
        IBrandRepository repository,
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
        var brand = await repository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException($"Brand '{brandName}' not found.");

        cache[brandName] = brand;
        return brand;
    }

}
