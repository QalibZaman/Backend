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

    public async Task<ICollection<CategoryEntity>> GetAllNested(Specification<CategoryEntity> specification, CancellationToken cancellationToken)
    {
        var response = await _dbSet.Where(specification.Criteria)
             .ToListAsync(cancellationToken);

        return response;
    }

    public async Task<int> GetAmount(CancellationToken cancellationToken)
    {
        var response = await _dbSet.CountAsync(cancellationToken);

        return response;
    }

    public async Task<ICollection<CategoryEntity>> GetAllPaginated(Specification<CategoryEntity> specification, CancellationToken cancellationToken)
    {
        var query = _dbSet.AsQueryable();

        foreach (var includeExpression in specification.Includes)
        {
            query = query.Include(includeExpression);
        }

        query = query.Skip(specification.Skip ?? 0).Take(specification.Take ?? _paginationDefaults.PageSize);

        var response = await query.ToListAsync(cancellationToken);

        return response;
    }
}
