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
    //public Expression<Func<TEntity, object>>? OrderBy { get; set; }
    //public Expression<Func<TEntity, object>>? OrderByDescending { get; set; }

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

    public Specification<TEntity> And(Specification<TEntity> next)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "p");

        var leftVisitor = new ReplaceParameterVisitor(Criteria.Parameters[0], parameter);
        var rightVisitor = new ReplaceParameterVisitor(next.Criteria.Parameters[0], parameter);

        var left = leftVisitor.Visit(Criteria.Body);
        var right = rightVisitor.Visit(next.Criteria.Body);
        Criteria = Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(left, right), parameter);
        return this;
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
}

public class ReplaceParameterVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _oldParameter;
    private readonly ParameterExpression _newParameter;

    public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
    {
        _oldParameter = oldParameter;
        _newParameter = newParameter;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _oldParameter ? _newParameter : base.VisitParameter(node);
    }
}
