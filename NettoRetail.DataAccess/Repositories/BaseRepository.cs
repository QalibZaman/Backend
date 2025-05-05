using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Shared.Extensions;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken)
    {
        _dbSet.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<TEntity?> GetAsync(Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        var query = _dbSet.Where(specification.Criteria);

        foreach (var include in specification.Includes)
        {
            query = query.Include(include);
        }

        var response = await query.FirstOrDefaultAsync(cancellationToken);

        return response;
    }

    public async Task<int> GetAmountAsync(CancellationToken cancellationToken)
    {
        var response = await _dbSet.CountAsync(cancellationToken);

        return response;
    }

    public async Task DeleteAsync(Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        var entityToDelete = await _dbSet.Where(specification.Criteria).FirstAsync(cancellationToken);
        _dbSet.Remove(entityToDelete);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetManyBySpecAsync(Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        var query = _dbSet.Where(specification.Criteria);

        foreach (var include in specification.Includes)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task DeleteManyBySpecAsync(Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        var entitiesToDelete = await _dbSet.Where(specification.Criteria).ToListAsync(cancellationToken);
        _dbSet.RemoveRange(entitiesToDelete);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        _dbSet.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllBySpecAsync(Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        var query = _dbSet.Where(specification.Criteria);

        foreach (var include in specification.Includes)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddBulkAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        await _dbSet.BulkInsertAsync(entities, cancellationToken);
    }

    public async Task<int> GetAmountBySpecAsync(Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(specification.Criteria).CountAsync(cancellationToken);
    }

    public async Task<bool> Exists(Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        return await _dbSet.AnyAsync(specification.Criteria, cancellationToken);
    }

    public async Task<(ICollection<TEntity> Items, int TotalCount)> GetAllPaginatedAsync(
            Specification<TEntity> specification,
            CancellationToken cancellationToken)
    {
        var query = _dbSet.AsQueryable();

        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        foreach (var include in specification.Includes)
        {
            query = query.Include(include);
        }

        if (specification.Orderings != null && specification.Orderings.Any())
        {
            var (expr, isDesc) = specification.Orderings.First();
            var orderedQuery = isDesc
                ? query.OrderByDescending(expr)
                : query.OrderBy(expr);

            foreach (var ordering in specification.Orderings.Skip(1))
            {
                orderedQuery = ordering.IsDesc
                    ? orderedQuery.ThenByDescending(ordering.Expr)
                    : orderedQuery.ThenBy(ordering.Expr);
            }

            query = orderedQuery;
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = query.ApplyPagination(specification);

        var items = await query.ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task UpdateBulkAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        await _dbSet.BulkUpdateAsync(entities, cancellationToken);
    }

    public async Task UpdateManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        _dbSet.UpdateRange(entities);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<TEntity> GetWithRelatedDataAsync(Specification<TEntity> specification, CancellationToken cancellationToken)
    {
        var query = _dbSet.Where(specification.Criteria);

        foreach (var include in specification.Includes)
        {
            query = query.Include(include);
        }

        var response = await query.FirstOrDefaultAsync(cancellationToken);

        return response!;
    }
}