using Hangfire;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Cart;
using NettoRetail.DataAccess.Specifications.CartItem;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.ICartService;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IInventoryItemRepository _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(
        ICartRepository cartRepository,
        ICartItemRepository cartItemRepository,
        IInventoryItemRepository inventoryRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
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

        var jobId = BackgroundJob.Schedule<CartService>(
            cs => cs.HardDeleteCartItemAsync(cart.Id, inventoryItemId, CancellationToken.None),
            TimeSpan.FromSeconds(10));

        cartItem.IsDeleted = true;
        cartItem.DeletionJobId = jobId;
        cart.LastUpdatedAt = DateTime.UtcNow;

        using var transaction = _unitOfWork.BeginTransaction();
        try
        {
            await _cartItemRepository.UpdateAsync(cartItem, cancellationToken);
            await _cartRepository.UpdateAsync(cart, cancellationToken);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task ClearCartItemsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId, cancellationToken) ?? throw new NotFoundException("Cart not found");

        if (!cart.Items.Any())
            throw new NotFoundException("No items in cart to clear");

        foreach (var item in cart.Items)
        {
            item.IsDeleted = true;
        }

        var jobId = BackgroundJob.Schedule<CartService>(
            cs => cs.HardDeleteAllCartItemsAsync(cart.Id, CancellationToken.None),
            TimeSpan.FromSeconds(10));

        cart.ItemsDeletionJobId = jobId;
        cart.LastUpdatedAt = DateTime.UtcNow;

        await _cartRepository.UpdateAsync(cart, cancellationToken);
    }

    public async Task<CartItemEntity> RestoreCartItemAsync(Guid customerId, Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId, cancellationToken) ?? throw new NotFoundException("Cart not found");

        var spec = new GetDeletedCartItemSpecification(cart.Id, inventoryItemId);
        var restoredCartItem = await _cartItemRepository.GetAsync(spec, cancellationToken) ?? throw new NotFoundException("Cart item for undo operation not found");

        var deletetionJobId = restoredCartItem.DeletionJobId;

        restoredCartItem.IsDeleted = false;
        restoredCartItem.DeletionJobId = null;
        cart.LastUpdatedAt = DateTime.UtcNow;

        using var transaction = _unitOfWork.BeginTransaction();
        try
        {
            await _cartItemRepository.UpdateAsync(restoredCartItem, cancellationToken);
            await _cartRepository.UpdateAsync(cart, cancellationToken);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        if (deletetionJobId != null)
            BackgroundJob.Delete(deletetionJobId);

        return restoredCartItem;
    }

    public async Task<IEnumerable<CartItemEntity>> RestoreAllCartItemsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId, cancellationToken) ?? throw new NotFoundException("Cart not found");

        var spec = new GetDeletedCartItemsSpecification(cart.Id);
        var restoredCartItems = await _cartItemRepository.GetManyBySpecAsync(spec, cancellationToken);

        if (!restoredCartItems.Any())
            throw new NotFoundException("No cart items available for undo operation");

        foreach (var item in restoredCartItems)
        {
            item.IsDeleted = false;
            item.DeletionJobId = null;
        }

        var deletetionJobId = cart.ItemsDeletionJobId;

        cart.ItemsDeletionJobId = null;
        cart.LastUpdatedAt = DateTime.UtcNow;

        using var transaction = _unitOfWork.BeginTransaction();
        try
        {
            await _cartItemRepository.UpdateManyAsync(restoredCartItems, cancellationToken);
            await _cartRepository.UpdateAsync(cart, cancellationToken);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        if (deletetionJobId != null)
            BackgroundJob.Delete(deletetionJobId);

        return restoredCartItems;
    }

    public async Task HardDeleteCartItemAsync(Guid cartId, Guid inventoryItemId, CancellationToken cancellationToken)
    {
        var spec = new GetDeletedCartItemSpecification(cartId, inventoryItemId);
        var cartItem = await _cartItemRepository.GetAsync(spec, cancellationToken);

        if (cartItem != null)
            await _cartItemRepository.DeleteAsync(cartItem, cancellationToken);
    }

    public async Task HardDeleteAllCartItemsAsync(Guid cartId, CancellationToken cancellationToken)
    {
        var cartSpec = new GetCartByIdSpecification(cartId);
        var cart = await _cartRepository.GetAsync(cartSpec, cancellationToken);

        if (cart == null)
            return;

        using var transaction = _unitOfWork.BeginTransaction();
        try
        {
            var spec = new GetDeletedCartItemsSpecification(cartId);
            await _cartItemRepository.DeleteManyBySpecAsync(spec, cancellationToken);

            cart.ItemsDeletionJobId = null;
            cart.LastUpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart, cancellationToken);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<CartEntity> AddManyItemsForUserAsync(Guid customerId, IEnumerable<Guid> inventoryItemIds, CancellationToken cancellationToken = default)
    {
        var ids = inventoryItemIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            throw new BadRequestException("No valid items to add.");

        var cart = await LoadCartAsync(customerId, cancellationToken);

        var inventorySpec = new Specification<InventoryItemEntity>(i => ids.Contains(i.Id));
        var products = await _inventoryRepository.GetManyBySpecAsync(inventorySpec, cancellationToken);

        var existingSpec = new Specification<CartItemEntity>(ci =>
            ci.CartId == cart.Id && ids.Contains(ci.InventoryItemId));
        var existing = await _cartItemRepository.GetManyBySpecAsync(existingSpec, cancellationToken);
        var existingIds = existing.Select(e => e.InventoryItemId).ToHashSet();

        var newItems = products
            .Where(p => !existingIds.Contains(p.Id))
            .Select(p => new CartItemEntity
            {
                CartId = cart.Id,
                InventoryItemId = p.Id,
                ProductUpc = p.ProductUpc,
                ProductName = p.ProductName,
                Price = p.Price,
                PromotionPrice = p.IsPromoted ? p.PromotionPrice : null,
                Quantity = 1
            })
            .ToList();

        if (newItems.Count == 0)
            throw new BadRequestException("No items were added (all unavailable or at maximum quantity).");

        await _cartItemRepository.AddRangeAsync(newItems, cancellationToken);

        cart.Items.AddRange(newItems);
        cart.LastUpdatedAt = DateTime.UtcNow;
        await _cartRepository.UpdateAsync(cart, cancellationToken);

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
            if (existingItem.IsDeleted)
            {
                existingItem.IsDeleted = false;
                existingItem.DeletionJobId = null;
                existingItem.Quantity = increment;
                existingItem.ProductName = product.ProductName;
                existingItem.Price = product.Price;
                existingItem.PromotionPrice = product.IsPromoted ? product.PromotionPrice : null;
            }
            else
            {
                var newQty = existingItem.Quantity + increment;
                if (newQty > product.AvailableAmount)
                    throw new BadRequestException("Quantity exceeds available stock.");

                existingItem.Quantity = newQty;
                existingItem.ProductName = product.ProductName;
                existingItem.Price = product.Price;
                existingItem.PromotionPrice = product.IsPromoted ? product.PromotionPrice : null;
            }

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
