using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class ShopRepository : BaseRepository<ShopEntity>, IShopRepository
{
    public ShopRepository(AppDbContext dbContext)
        : base(dbContext) { }
}
