using Microsoft.EntityFrameworkCore;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class BrandRepository : BaseRepository<BrandEntity>, IBrandRepository
{
    public BrandRepository(AppDbContext dbContext)
    : base(dbContext) { }
    public async Task<string> GetBrandNameAsync(int id, int shopId, CancellationToken cancellationToken)
    {
        var name = await _dbSet
            .AsNoTracking()
            .Where(b => b.Id == id && b.ShopId == shopId)
            .Select(b => b.Name)
            .SingleOrDefaultAsync(cancellationToken);

        return name;
    }
}
