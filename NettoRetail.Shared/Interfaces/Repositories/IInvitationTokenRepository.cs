using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IInvitationTokenRepository : IBaseRepository<InvitationTokenEntity>
{
    Task<InvitationTokenEntity?> GetByHashAsync(Specification<InvitationTokenEntity> specifications, CancellationToken cancellationToken);

    Task InvalidateAllActiveAsync(Guid AccountId,CancellationToken cancellationToken);
}