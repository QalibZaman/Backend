using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IProductDetailRepository : IBaseRepository<ProductDetailEntity>
{
    Task UpdateDetailsAsync(IEnumerable<ProductDetailEntity> upsertedDetails, CancellationToken cancellationToken);
    Task DeleteRangeAsync(
        IEnumerable<ProductDetailEntity> detailsToDelete,
        CancellationToken cancellationToken);
}