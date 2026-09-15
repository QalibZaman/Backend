using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories
{
    public class RoleRepository(AppDbContext dbContext) : BaseRepository<RoleEntity>(dbContext), IRoleRepository
    {
        public async Task<List<RoleEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await dbContext.ApplicationRoles.ToListAsync(cancellationToken);
        }

        public async Task<List<RoleEntity>> GetRolesWithPermissionsBySpec(Specification<RoleEntity> specification, CancellationToken cancellationToken)
        {
            var roles = await dbContext.ApplicationRoles
                .Where(specification.Criteria)
                .Include(r => r.PermissionRoles)
                    .ThenInclude(pr => pr.Permission)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return roles;
        }
    }

}
