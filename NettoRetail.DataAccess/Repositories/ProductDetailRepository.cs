using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class ProductDetailRepository(AppDbContext dbContext)
    : BaseRepository<ProductDetailEntity>(dbContext), IProductDetailRepository
{
    public async Task UpdateDetailsAsync(
        IEnumerable<ProductDetailEntity> upsertedDetails,
        CancellationToken cancellationToken)
    {
        _dbSet.UpdateRange(upsertedDetails);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRangeAsync(
        IEnumerable<ProductDetailEntity> detailsToDelete,
        CancellationToken cancellationToken)
    {
        _dbContext.RemoveRange(detailsToDelete);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}