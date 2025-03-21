using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories;

public class PermissionRepository(AppDbContext dbContext)
    : BaseRepository<PermissionEntity>(dbContext),
    IPermissionRepository
{
    public async Task<IEnumerable<int>> GetIdsBySpecAsync(Specification<PermissionEntity> specification, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(specification.Criteria).Select(permission => permission.Id).ToListAsync();
    }
}
