using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class RoleRepository(AppDbContext dbContext, IOptions<PaginationOptions> paginationOptions)
    : BaseRepository<RoleEntity>(dbContext),
    IRoleRepository
{
    private PaginationOptions _paginationDefaults = paginationOptions.Value;
    public async Task<ICollection<RoleEntity>> GetAllPaginatedAsync(Specification<RoleEntity> specification, CancellationToken cancellationToken)
    {
        var query = _dbSet.AsQueryable();

        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        foreach (var includeExpression in specification.Includes)
        {
            query = query.Include(includeExpression);
        }

        query = query.Skip(specification.Skip ?? 0)
                     .Take(specification.Take ?? _paginationDefaults.PageSize);

        var response = await query.ToListAsync(cancellationToken);

        return response;
    }
}