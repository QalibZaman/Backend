using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IOrderRepository : IBaseRepository<OrderEntity>
{
    public Task<OrderEntity?> GetOrderForPrinting(Specification<OrderEntity> specification, CancellationToken);
}