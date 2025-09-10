using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.ICartService
{
    public interface ICartService
    {
        Task<CartEntity> GetCartAsync(Guid customerId, CancellationToken cancellationToken);

        Task<CartEntity> AddItemForUserAsync(
            Guid customerId,
            CartItemEntity item,
            CancellationToken cancellationToken = default);

        Task<bool> IsProductAvailableAsync(
            long productUpc,
            CancellationToken cancellationToken);
    }
}
