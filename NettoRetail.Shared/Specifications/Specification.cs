using System.Linq.Expressions;

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
    private readonly List<Expression<Func<TEntity, object>>> _includes;
    public IEnumerable<Expression<Func<TEntity, object>>> Includes => _includes;

    // Pagination
    public int? Skip { get; set; }
    public int? Take { get; set; }

    public Specification(Expression<Func<TEntity, bool>> criteria)
    {
        Criteria = criteria;
        _includes = new List<Expression<Func<TEntity, object>>>();
        _orderings = new List<(Expression<Func<TEntity, object>>, bool)>();
    }

    public void And(Specification<TEntity> other)
    {
        var combinedExpression = Expression.Lambda<Func<TEntity, bool>>(
            Expression.AndAlso(
                Criteria.Body,
                Expression.Invoke(other.Criteria, Criteria.Parameters[0])),
            Criteria.Parameters[0]);

        this.Criteria = combinedExpression;
    }

    public Specification<TEntity> AddInclude(Expression<Func<TEntity, object>> includeExpression)
    {
        _includes.Add(includeExpression);
        return this;
    }

    public Specification<TEntity> AddOrderBy(Expression<Func<TEntity, object>> orderByExpression, bool descending = false)
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
