using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IInventoryItemRepository _inventoryRepository;

    public CartService(ICartRepository cartRepository, IInventoryItemRepository inventoryRepository)
    {
        _cartRepository = cartRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<CartEntity> GetCartAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        if (cart == null)
            throw new NotFoundException("Cart not found.");

        cart.Items ??= new List<CartItemEntity>();
        return cart;
    }

    public async Task<CartEntity> AddItemForUserAsync(Guid customerId, CartItemEntity item, CancellationToken cancellationToken = default)
    {
        var product = await ValidateProductAsync(item, cancellationToken);
        var cart = await LoadCartAsync(customerId, cancellationToken);
        AddOrUpdateCartItem(cart, item, product);
        await SaveCartAsync(cart, cancellationToken);
        return cart;
    }

    public async Task<bool> IsProductAvailableAsync(long productUpc, CancellationToken cancellationToken)
    {
        var product = await _inventoryRepository.GetByUpcAsync(productUpc, cancellationToken);

        return product != null
               && product.Status == InventoryItemStatusEnum.Available
               && product.AvailableAmount > 0;
    }

    private async Task<InventoryItemEntity> ValidateProductAsync(CartItemEntity item, CancellationToken cancellationToken)
    {
        var product = await _inventoryRepository.GetByUpcAsync(item.ProductUpc, cancellationToken);
        if (product == null || product.Status != InventoryItemStatusEnum.Available || product.AvailableAmount <= 0)
            throw new Exception("Product is out of stock.");

        if (item.Quantity <= 0 || item.Quantity > product.AvailableAmount)
            throw new Exception("Invalid quantity.");

        if (item.Price != product.Price)
            throw new Exception("Price mismatch.");

        if (item.PromotionPrice.HasValue && item.PromotionPrice.Value > product.Price)
            throw new Exception("Promotion price cannot exceed original product price.");

        return product;
    }

    private async Task<CartEntity> LoadCartAsync(Guid customerId, CancellationToken cancellationToken)
    {
        return await _cartRepository.GetByCustomerIdAsync(customerId, cancellationToken)
               ?? new CartEntity
               {
                   Id = Guid.NewGuid(),
                   CustomerId = customerId,
                   Items = new List<CartItemEntity>()
               };
    }

    private void AddOrUpdateCartItem(CartEntity cart, CartItemEntity item, InventoryItemEntity product)
    {
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductUpc == item.ProductUpc && i.Size == item.Size);
        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + item.Quantity;
            if (newQuantity > product.AvailableAmount)
                throw new Exception("Cannot add more than available stock.");

            existingItem.Quantity = newQuantity;
            existingItem.Price = product.Price;
            existingItem.PromotionPrice = item.PromotionPrice;
        }
        else
        {
            item.Id = Guid.NewGuid();
            item.Price = product.Price;
            cart.Items.Add(item);
        }

        cart.LastUpdatedAt = DateTime.UtcNow;
    }

    private async Task SaveCartAsync(CartEntity cart, CancellationToken cancellationToken)
    {
        var exists = await _cartRepository.GetByIdAsync(cart.Id, cancellationToken);
        if (exists == null)
            await _cartRepository.AddAsync(cart, cancellationToken);
        else
            await _cartRepository.UpdateAsync(cart, cancellationToken);
    }
}
