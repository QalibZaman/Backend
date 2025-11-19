using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories
{
    public interface ICartRepository : IBaseRepository<CartEntity>
    {
        Task<CartEntity?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);
        Task<CartEntity?> GetCartWithItemsAsync(Guid customerId, CancellationToken cancellationToken);

    }
}
