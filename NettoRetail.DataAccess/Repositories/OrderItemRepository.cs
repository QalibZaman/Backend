using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories;

public class OrderItemRepository : BaseRepository<OrderItemEntity>, IOrderItemRepository
{
    public OrderItemRepository(AppDbContext context)
        : base(context)
    {
    }
}
