using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class ProductDetailRepository(AppDbContext dbContext)
    : BaseRepository<ProductDetailEntity>(dbContext), IProductDetailRepository
{
    public async Task UpdateDetailsAsync(
        ICollection<ProductDetailEntity> upsertedDetails,
        ICollection<Guid> deleteIds,
        CancellationToken cancellationToken)
    {
        _dbSet.UpdateRange(upsertedDetails);

        if (deleteIds != null)
        {
            await _dbSet.Where(d => deleteIds.Contains(d.Id))
            .ExecuteDeleteAsync();
        }

        await _dbContext.SaveChangesAsync();


    }
}