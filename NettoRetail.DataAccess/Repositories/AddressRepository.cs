using Microsoft.Azure.Cosmos.Linq;
using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class AddressRepository(AppDbContext dbContext) : BaseRepository<AddressEntity>(dbContext), IAddressRepository
{
    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var count = await dbContext.Addresses.CountAsync(x => x.UserId == userId, cancellationToken);

        return count;
    }

    public async Task<AddressEntity?> GetFallbackAddressAsync(Guid userId, Guid excludedId, CancellationToken cancellationToken)
    {
        return await dbContext.Addresses
            .Where(pm => pm.UserId == userId && pm.Id != excludedId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
