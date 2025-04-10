using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories
{
    public class PermissionRoleRepsitory(AppDbContext dbContext) : BaseRepository<PermissionRoleEntity>(dbContext), IPermissionRoleRepository
    {
        public async Task<IEnumerable<string>> GetPermissionsByRoleIdAsync(Specification<PermissionRoleEntity> specification, CancellationToken cancellationToken)
        {
            var query = _dbSet.AsQueryable();

            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            foreach (var include in specification.Includes) {
                query = query.Include(include);
            }

            return await query.Select(model => model.Permission.Title).ToListAsync(cancellationToken);
        }
    }
}
