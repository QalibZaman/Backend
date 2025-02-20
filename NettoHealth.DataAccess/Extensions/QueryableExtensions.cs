using Microsoft.EntityFrameworkCore;
using NettoHealth.Shared.Specifications;
using System.Linq.Expressions;

namespace NettoHealth.DataAccess.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySpecification<T>(this IQueryable<T> query, Specification<T> specification)
        where T : class 
    {
        if (specification.Includes.Any())
        {
            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));
        }

        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        if (specification.Skip.HasValue)
        {
            query = query.Skip(specification.Skip.Value);
        }

        if (specification.Take.HasValue)
        {
            query = query.Take(specification.Take.Value);
        }

        return query;
    }
}