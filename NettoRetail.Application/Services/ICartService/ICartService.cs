using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.ICartService
{
    public interface ICartService
    {
        Task<CartEntity> AddItemForUserAsync(
            Guid customerId,
            Guid inventoryItemId,
            CancellationToken cancellationToken = default);

        Task<CartItemEntity> UpdateItemQuantityAsync(
            Guid customerId,
            Guid cartItemId,
            int newQuantity,
            CancellationToken cancellationToken = default);

        Task DeleteCartItemAsync(
            Guid customerId,
            Guid inventoryItemId,
            CancellationToken cancellationToken = default);

        Task ClearCartItemsAsync(
            Guid customerId,
            CancellationToken cancellationToken = default);

        Task<CartItemEntity> RestoreCartItemAsync(
            Guid customerId,
            Guid inventoryItemId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<CartItemEntity>> RestoreAllCartItemsAsync(
            Guid customerId,
            CancellationToken cancellationToken = default);

        Task<CartEntity> AddManyItemsForUserAsync(
            Guid customerId,
            IEnumerable<Guid> inventoryItemIds,
            CancellationToken cancellationToken = default);
    }
}
