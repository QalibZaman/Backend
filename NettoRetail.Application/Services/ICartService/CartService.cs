using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.CartItem;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.ICartService;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IInventoryItemRepository _inventoryRepository;

    public CartService(
        ICartRepository cartRepository,
        ICartItemRepository cartItemRepository,
        IInventoryItemRepository inventoryRepository)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<CartEntity> AddItemForUserAsync(
        Guid customerId,
        Guid inventoryItemId,
        CancellationToken cancellationToken = default)
    {
        const int increment = 1;

        var product = await ValidateProductAsync(inventoryItemId, increment, cancellationToken);

        var cart = await LoadCartAsync(customerId, cancellationToken);

        await AddOrUpdateCartItemAsync(cart, product, requestedSize: null, increment, cancellationToken);

        return cart;
    }

    public async Task<CartItemEntity> UpdateItemQuantityAsync(Guid customerId, Guid cartItemId, int newQuantity, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId, cancellationToken)
           ?? throw new NotFoundException("Cart not found.");

        var cartItem = cart.Items.FirstOrDefault(x => x.Id == cartItemId)
           ?? throw new NotFoundException("Cart item not found.");

        if (newQuantity < 1)
            throw new BadRequestException("Quantity must be at least 1");

        var specification = new GetInventoryItemByIdSpecification(cartItem.InventoryItemId);

        var product = await _inventoryRepository.GetAsync(specification, cancellationToken);

        if (product == null || product.Status != InventoryItemStatusEnum.Available)
            throw new BadRequestException("Product is not available");

        if (newQuantity > product.AvailableAmount)
            throw new BadRequestException($"Only {product.AvailableAmount} items available in stock.");

        cartItem.Quantity = newQuantity;
        cartItem.Price = product.Price;
        cartItem.PromotionPrice = product.IsPromoted ? product.PromotionPrice : null;

        cart.LastUpdatedAt = DateTime.UtcNow;

        await _cartRepository.UpdateAsync(cart, cancellationToken);

        return cartItem;
    }

    public async Task DeleteCartItemAsync(Guid customerId, Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId, cancellationToken) ?? throw new NotFoundException("Cart not found");

        var spec = new GetCartItemByCartAndItemIdSpecification(cart.Id, inventoryItemId);
        var cartItem = await _cartItemRepository.GetAsync(spec, cancellationToken) ?? throw new NotFoundException("Item not found in cart");

        await _cartItemRepository.DeleteAsync(cartItem, cancellationToken);

        cart.LastUpdatedAt = DateTime.UtcNow;
        await _cartRepository.UpdateAsync(cart, cancellationToken);
    }

    public async Task ClearCartItemsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId, cancellationToken) ?? throw new NotFoundException("Cart not found");

        var spec = new GetCartItemsByCartIdSpecification(cart.Id);
        await _cartItemRepository.DeleteManyBySpecAsync(spec, cancellationToken);

        cart.LastUpdatedAt = DateTime.UtcNow;
        await _cartRepository.UpdateAsync(cart, cancellationToken);
    }

    private async Task<InventoryItemEntity> ValidateProductAsync(
        Guid inventoryItemId,
        int increment,
        CancellationToken cancellationToken)
    {
        var spec = new GetInventoryItemByIdSpecification(inventoryItemId);
        var product = await _inventoryRepository.GetAsync(spec, cancellationToken);

        if (product == null)
            throw new NotFoundException("Product not found.");

        if (product.Status != InventoryItemStatusEnum.Available || product.AvailableAmount <= 0)
            throw new BadRequestException("Product is out of stock.");

        if (increment <= 0 || increment > product.AvailableAmount)
            throw new BadRequestException("Invalid quantity.");

        return product;
    }

    private async Task<CartEntity> LoadCartAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var existing = await _cartRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        if (existing != null)
            return existing;

        var newCart = new CartEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Items = new List<CartItemEntity>(),
            LastUpdatedAt = DateTime.UtcNow
        };

        await _cartRepository.AddAsync(newCart, cancellationToken);
        return newCart;
    }

    private async Task AddOrUpdateCartItemAsync(
        CartEntity cart,
        InventoryItemEntity product,
        string? requestedSize,
        int increment,
        CancellationToken cancellationToken)
    {
        var size = requestedSize;

        var spec = new GetCartItemByCartAndInventorySpecification(cart.Id, product.Id, size);
        var existingItem = await _cartItemRepository.GetAsync(spec, cancellationToken);

        cart.LastUpdatedAt = DateTime.UtcNow;

        if (existingItem != null)
        {
            var newQty = existingItem.Quantity + increment;
            if (newQty > product.AvailableAmount)
                throw new BadRequestException("Quantity exceeds available stock.");

            existingItem.Quantity = newQty;
            existingItem.ProductName = product.ProductName;
            existingItem.Price = product.Price;
            existingItem.PromotionPrice = product.IsPromoted ? product.PromotionPrice : null;

            await _cartItemRepository.UpdateAsync(existingItem, cancellationToken);
        }
        else
        {
            if (increment > product.AvailableAmount)
                throw new BadRequestException("Quantity exceeds available stock.");

            var newItem = new CartItemEntity
            {
                CartId = cart.Id,
                InventoryItemId = product.Id,
                ProductUpc = product.ProductUpc,
                ProductName = product.ProductName,
                Price = product.Price,
                PromotionPrice = product.IsPromoted ? product.PromotionPrice : null,
                Quantity = increment,
                Size = size
            };

            await _cartItemRepository.AddAsync(newItem, cancellationToken);
        }
    }
}
