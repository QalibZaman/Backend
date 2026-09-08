using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Repositories
{
    public class RoleRepository(AppDbContext dbContext) : BaseRepository<RoleEntity>(dbContext), IRoleRepository
    {
        public async Task<List<RoleEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await dbContext.ApplicationRoles.ToListAsync(cancellationToken);
        }
    }
}