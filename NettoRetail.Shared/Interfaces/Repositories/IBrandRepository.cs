using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IBrandRepository : IBaseRepository<BrandEntity>
{
    Task<string> GetBrandNameAsync(int brandId, int shopId, CancellationToken cancellationToken);
}
