using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.DataAccess.Specifications.Inventory;
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

    public async Task<CartEntity> AddItemForUserAsync(Guid customerId, Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        const int increment = 1;

        var product = await ValidateProductAsync(inventoryItemId, increment, cancellationToken);

        var cart = await LoadCartAsync(customerId, cancellationToken);

        AddOrUpdateCartItem(cart, product, null, increment);

        await SaveCartAsync(cart, cancellationToken);

        return cart;
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

        return new CartEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Items = new List<CartItemEntity>()
        };
    }

    private void AddOrUpdateCartItem(CartEntity cart, InventoryItemEntity product, string? requestedSize, int increment)
    {
        var size = requestedSize;

        var existingItem = cart.Items.FirstOrDefault(i => i.InventoryItemId == product.Id && i.Size == size);

        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + increment;
            existingItem.Quantity = newQuantity;
            existingItem.ProductName = product.ProductName;
            existingItem.Price = product.Price;
            existingItem.PromotionPrice = product.IsPromoted ? product.PromotionPrice : null;
        }
        else
        {
            cart.Items.Add(new CartItemEntity
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                InventoryItemId = product.Id,
                InventoryItem = product,

                ProductUpc = product.ProductUpc,
                ProductName = product.ProductName,
                Price = product.Price,
                PromotionPrice = product.IsPromoted ? product.PromotionPrice : null,
                Quantity = increment,
                Size = size
            });
        }

        cart.LastUpdatedAt = DateTime.UtcNow;
    }

    private async Task SaveCartAsync(CartEntity cart, CancellationToken cancellationToken)
    {
        var existing = await _cartRepository.GetByCustomerIdAsync(cart.CustomerId, cancellationToken);

        if (existing == null)
        {
            await _cartRepository.AddAsync(cart, cancellationToken);
        }
        else
        {
            cart.Id = existing.Id;
            await _cartRepository.UpdateAsync(cart, cancellationToken);
        }
    }
}
