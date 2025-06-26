using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories
{
    public interface IFeatureRepository : IBaseRepository<FeatureEntity>
    {
        Task<IEnumerable<FeatureEntity>> GetAllByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    }
}