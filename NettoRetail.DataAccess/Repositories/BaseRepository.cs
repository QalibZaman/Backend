using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity>
    where TEntity : class
{
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly AppDbContext _dbContext;

    protected BaseRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<TEntity>();
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<TEntity?> GetAsync(Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        var response = await _dbSet.Where(specification.Criteria)
            .FirstOrDefaultAsync(cancellationToken);

        return response;
    }

    public async Task DeleteAsync(Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        var entitityToDelete = await _dbSet.Where(specification.Criteria).FirstAsync(cancellationToken);
        _dbSet.Remove(entitityToDelete);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetManyBySpecAsync(Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(specification.Criteria).ToListAsync(cancellationToken);
    }

    public async Task DeleteManyBySpecAsync(Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        var entitiesToDelete = await _dbSet.Where(specification.Criteria).ToListAsync();
        _dbSet.RemoveRange(entitiesToDelete);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        _dbSet.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
