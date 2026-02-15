using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace NettoRetail.Shared.Specifications;

public class Specification<TEntity>
    where TEntity : class
{
    // Instead of writing an empty expression,
    // we can isolate the logic into a static property and use it if need be.
    public static Expression<Func<TEntity, bool>> Empty => _ => true;
    // Main expression
    public Expression<Func<TEntity, bool>> Criteria { get; set; }
    public Expression<Func<TEntity, object>> Select { get; set; }

    // Ordering expressions
    private readonly List<(Expression<Func<TEntity, object>>, bool)> _orderings;
    public IEnumerable<(Expression<Func<TEntity, object>> Expr, bool IsDesc)> Orderings => _orderings;

    // Includes
    [Obsolete("Use _includes to get support for ThenInclude functionality.")]
    private readonly List<Expression<Func<TEntity, object>>> _obsoleteIncludes;

    [Obsolete("Use UpdatedIncludes to get support for ThenInclude functionality.")]
    public IEnumerable<Expression<Func<TEntity, object>>> Includes => _obsoleteIncludes;

    private List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> _includes;
    public IEnumerable<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> UpdatedIncludes => _includes;

    // Pagination
    public int? Skip { get; set; }
    public int? Take { get; set; }
	public Specification(Expression<Func<TEntity, bool>> criteria)
    {
        Criteria = criteria;
        _obsoleteIncludes = new List<Expression<Func<TEntity, object>>>();
        _orderings = new List<(Expression<Func<TEntity, object>>, bool)>();
        _includes = new List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>>();
    }

    public Specification<TEntity> And(Specification<TEntity> other)
    {
        var combinedExpression = Expression.Lambda<Func<TEntity, bool>>(
            Expression.AndAlso(
                Criteria.Body,
                Expression.Invoke(other.Criteria, Criteria.Parameters[0])),
            Criteria.Parameters[0]);

        this.Criteria = combinedExpression;

        return this;
    }

    public Specification<TEntity> Or(Specification<TEntity> other)
    {
        var combinedExpression = Expression.Lambda<Func<TEntity, bool>>(
            Expression.OrElse(
                Criteria.Body,
                Expression.Invoke(other.Criteria, Criteria.Parameters[0])),
            Criteria.Parameters[0]);

        this.Criteria = combinedExpression;

        return this;
    }

    [Obsolete("Use override that accepts Func with IQueryable. To get support for ThenInclude.")]
    public Specification<TEntity> AddInclude(Expression<Func<TEntity, object>> includeExpression)
    {
        _obsoleteIncludes.Add(includeExpression);
        return this;
    }

    public Specification<TEntity> AddInclude(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includeExpression)
    {
        _includes.Add(includeExpression);
        return this;
    }

    public Specification<TEntity> AddOrderBy(Expression<Func<TEntity, object>> orderByExpression, bool descending = false)
    {
        _orderings.Add((orderByExpression, descending));
        return this;
    }
    protected void ApplyPaging(int pageIndex, int pageSize)
    {
        Skip = (pageIndex - 1) * pageSize;
        Take = pageSize;
    }

    public Specification<TEntity> AddOrderByDescending(Expression<Func<TEntity, object>> orderByExpression, bool descending = true)
    {
        _orderings.Add((orderByExpression, descending));
        return this;
    }

    public Specification<TEntity> AddSelectExpression(Expression<Func<TEntity, object>> selectExpression)
    {
        Select = selectExpression;
        return this;
    }

    public Specification<TEntity> AddCriteria(Expression<Func<TEntity, bool>> newCriteria)
    {
        var combined = Expression.Lambda<Func<TEntity, bool>>(
            Expression.AndAlso(
                Criteria.Body,
                Expression.Invoke(newCriteria, Criteria.Parameters[0])),
            Criteria.Parameters[0]);

        Criteria = combined;
        return this;
    }
}
