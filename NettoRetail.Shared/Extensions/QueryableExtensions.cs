using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, Specification<T> specification)
        where T : class
    {
        if (specification.Skip.HasValue && specification.Take.HasValue)
        {
            query = query.Skip(specification.Skip.Value).Take(specification.Take.Value);
        }

        return query;
    }
}