using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IOrderStatusHistoryRepository : IBaseRepository<OrderStatusHistoryEntity>
{
    Task<IEnumerable<OrderStatusHistoryEntity>> GetOrderHistoryIncludingAccount(int id, CancellationToken cancellationToken);
}
