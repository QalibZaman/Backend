using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IShopRepository : IBaseRepository<ShopEntity>
{
    public Task<ShopEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
