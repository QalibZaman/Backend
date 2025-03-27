using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class ProductDetailsRepository : IProductDetailsRepository
{
    protected readonly DbSet<ProductDetailsEntity> _dbSet;
    protected readonly CosmosDbContext _dbContext;

    public ProductDetailsRepository(CosmosDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<ProductDetailsEntity>();
    }

    public async Task<ProductDetailsEntity> AddAsync(ProductDetailsEntity entity, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task DeleteAsync(ProductDetailsEntity entity, CancellationToken cancellationToken)
    {
        _dbSet.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductDetailsEntity?> GetAsync(Specification<ProductDetailsEntity> specification, CancellationToken cancellationToken)
    {
        var response = await _dbSet.Where(specification.Criteria)
            .FirstOrDefaultAsync(cancellationToken);

        return response;
    }

    public async Task<IEnumerable<ProductDetailsEntity>> GetManyBySpecAsync(Specification<ProductDetailsEntity> specification, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(specification.Criteria).ToListAsync(cancellationToken);
    }

    public async Task DeleteManyBySpecAsync(Specification<ProductDetailsEntity> specification, CancellationToken cancellationToken)
    {
        var entitiesToDelete = await _dbSet.Where(specification.Criteria).ToListAsync();
        _dbSet.RemoveRange(entitiesToDelete);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ProductDetailsEntity entity, CancellationToken cancellationToken)
    {
        _dbSet.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
