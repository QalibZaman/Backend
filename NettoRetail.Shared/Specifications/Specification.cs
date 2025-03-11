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

    // Ordering expressions
    public Expression<Func<TEntity, object>>? OrderBy { get; set; }
    public Expression<Func<TEntity, object>>? OrderByDescending { get; set; }

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
    }

    public Specification<TEntity> And(Specification<TEntity> next)
    {
        var combinedExpression = Expression.Lambda<Func<TEntity, bool>>(
            Expression.AndAlso(Criteria.Body, Expression.Invoke(next.Criteria, Criteria.Parameters[0])));

        return new Specification<TEntity>(combinedExpression);
    }

    public Specification<TEntity> AddInclude(Expression<Func<TEntity, object>> includeExpression)
    {
        _includes.Add(includeExpression);
        return this;
    }
}