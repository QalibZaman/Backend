using NettoRetail.Domain.Entities;
using NettoRetail.DataAccess.Context;
using NettoRetail.Shared.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace NettoRetail.DataAccess.Repositories
{
    public class FeatureRepository : BaseRepository<FeatureEntity>, IFeatureRepository
    {
        public FeatureRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<FeatureEntity>> GetAllByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(f => ids.Contains(f.Id))
                .ToListAsync(cancellationToken);
        }
    }
}
