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

    public async Task<bool> TryLockAsync(
        int roleId,
        int shopId,
        Guid userId,
        DateTime nowUtc,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        var ttlBorder = nowUtc - ttl;

        var rows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $@"
            UPDATE Roles
            SET LockedByUserId = {userId},
                LockedAtUtc     = {nowUtc}
            WHERE Id = {roleId}
              AND ShopId = {shopId}
              AND (
                    LockedByUserId IS NULL
                 OR LockedAtUtc < {ttlBorder}
                 OR LockedByUserId = {userId}
              )
        ",
            cancellationToken);

        return rows == 1;
    }

    public async Task<bool> RefreshLockAsync(
        int roleId,
        int shopId,
        Guid userId,
        DateTime nowUtc,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        var ttlBorder = nowUtc - ttl;

        var rows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $@"
            UPDATE Roles
            SET LockedAtUtc = {nowUtc}
            WHERE Id = {roleId}
              AND ShopId = {shopId}
              AND LockedByUserId = {userId}
              AND LockedAtUtc >= {ttlBorder}
        ",
            cancellationToken);

        return rows == 1;
    }

    public async Task<bool> UnlockAsync(int roleId, int shopId, Guid userId, CancellationToken cancellationToken)
    {
        var rows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $@"
            UPDATE Roles
            SET LockedByUserId = NULL,
                LockedAtUtc     = NULL
            WHERE Id = {roleId}
              AND ShopId = {shopId}
              AND LockedByUserId = {userId}
        ",
            cancellationToken);

        return rows == 1;
    }
}
