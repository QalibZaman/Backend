using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories
{
    public interface ICommunicationPreferencesRepository : IBaseRepository<CommunicationPreferenceEntity>
    {
        public Task<CommunicationPreferenceEntity?> GetByUserIdAsync(Specification<CommunicationPreferenceEntity> specification, CancellationToken cancellationToken);
    }
}
