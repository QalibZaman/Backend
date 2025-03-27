using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class CategoryRepository(AppDbContext dbContext, IOptions<PaginationOptions> paginationOptions)
    : BaseRepository<CategoryEntity>(dbContext), ICategoryRepository
{
    private PaginationOptions _paginationDefaults = paginationOptions.Value;

    public async Task<ICollection<CategoryEntity>> GetAllNestedAsync(Specification<CategoryEntity> specification, CancellationToken cancellationToken)
    {
        var response = await _dbSet.Where(specification.Criteria)
             .ToListAsync(cancellationToken);

        return response;
    }

    public async Task<ICollection<CategoryEntity>> GetAllPaginatedAsync(Specification<CategoryEntity> specification, CancellationToken cancellationToken)
    {
        var query = _dbSet.AsQueryable();

        foreach (var includeExpression in specification.Includes)
        {
            query = query.Include(includeExpression);
        }

        query = query.Skip(specification.Skip ?? 0).Take(specification.Take ?? _paginationDefaults.PageSize);

        var sql = query.ToQueryString();

        var response = await query.ToListAsync(cancellationToken);

        return response;
    }

    public async Task<List<CategoryEntity>> GetCategorySubtreeAsync(int categoryId, CancellationToken cancellationToken)
    {
        var query = @"
            WITH RECURSIVE CategoriesSubtree AS (
                SELECT * FROM public.""Categories"" WHERE ""Id"" = @p0
                UNION ALL
                SELECT c.* FROM public.""Categories"" c
                INNER JOIN CategoriesSubtree rc ON c.""ParentId"" = rc.""Id""
            )
            SELECT * FROM CategoriesSubtree;
        ";

        return await _dbContext.Categories.FromSqlRaw(query, categoryId).ToListAsync(cancellationToken);
    }

    public async Task<List<CategoryEntity>> GetCategoryWithHierarchyAsync(int categoryId, CancellationToken cancellationToken)
    {
        var sql = @"
            WITH RECURSIVE CategoriesSubtree AS (
                SELECT c.""Id"", c.""Name"", c.""ParentId"", c.""URL"", c.""Visibility"", c.""Level"", c.""ShopId""
                FROM public.""Categories"" c WHERE c.""Id"" = @p0

                UNION ALL

                SELECT c.""Id"", c.""Name"", c.""ParentId"", c.""URL"", c.""Visibility"", c.""Level"", c.""ShopId""
                FROM public.""Categories"" c
                INNER JOIN CategoriesSubtree rc ON c.""Id"" = rc.""ParentId""
            )
            SELECT * FROM CategoriesSubtree
            ORDER BY ""Level"" ASC;";

        return await _dbSet.FromSqlRaw(sql, categoryId).ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetCategoryNameAndIdDictionaryAsync(CancellationToken cancellationToken)
    {
        var result = await _dbSet.Select(x => new { x.Name, x.Id }).ToDictionaryAsync(x => x.Name, x => x.Id, cancellationToken);

        return result;
    }
}
