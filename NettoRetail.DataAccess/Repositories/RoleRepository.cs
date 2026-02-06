using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class RoleRepository(AppDbContext dbContext)
    : BaseRepository<RoleEntity>(dbContext),
    IRoleRepository
{
    public async Task<List<RoleEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Roles.ToListAsync(cancellationToken);
    }

    public async Task<RoleEntity> AttachOrAddAsync(RoleEntity role, CancellationToken cancellationToken)
    {
        if (role.Id != 0)
        {
            _dbContext.Attach(role);
        }
        else
        {
            await _dbContext.Roles.AddAsync(role, cancellationToken);
        }

        return role;
    }
}
