using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class ProductDetailRepository(AppDbContext dbContext)
    : BaseRepository<ProductDetailEntity>(dbContext), IProductDetailRepository
{
    public async Task UpdateDetailsAsync(
        Specification<ProductDetailEntity> upsertSpecification,
        Specification<ProductDetailEntity> deleteSpecification,
        CancellationToken cancellationToken)
    {
        await _dbSet.Where(upsertSpecification.Criteria).ExecuteUpdate();
    }
}