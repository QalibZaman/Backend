using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.IBranService;

public interface IBrandService
{
    Task<BrandEntity> GetOrAddBrandAsync(
        IBrandRepository repository,
        Dictionary<string, BrandEntity> cache,
        string brandName,
        int shopId,
        CancellationToken cancellationToken);
}
