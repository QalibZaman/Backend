using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IProductDetailRepository : IBaseRepository<ProductDetailEntity>
{
    Task UpdateDetailsAsync(ICollection<ProductDetailEntity> upsertedDetails, ICollection<Guid> deleteIds, CancellationToken cancellationToken = default);
}