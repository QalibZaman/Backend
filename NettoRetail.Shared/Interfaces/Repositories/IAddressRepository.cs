using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IAddressRepository : IBaseRepository<AddressEntity>
{
    Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<AddressEntity?> GetFallbackAddressAsync(Guid userId, Guid excludedId, CancellationToken cancellationToken);
}
