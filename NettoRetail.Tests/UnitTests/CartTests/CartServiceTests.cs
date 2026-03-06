using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.DataAccess.Specifications.CartItem;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.CartServiceTest
{
    public class CartServiceTests
    {
        private readonly ICartRepository _cartRepo;
        private readonly IInventoryItemRepository _invRepo;
        private readonly ICartItemRepository _cartItemRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _cartRepo = Substitute.For<ICartRepository>();
            _invRepo = Substitute.For<IInventoryItemRepository>();
            _cartItemRepo = Substitute.For<ICartItemRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();

            _cartService = new CartService(
                cartRepository: _cartRepo,
                cartItemRepository: _cartItemRepo,
                inventoryRepository: _invRepo,
                unitOfWork: _unitOfWork

            );
        }

        [Fact]
        public async Task AddItemForUserAsync_AddsNewItem_WhenCartDoesNotExist()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(5);

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(product);

            _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>())
                .Returns((CartEntity?)null);

            _cartItemRepo.GetAsync(Arg.Any<GetCartItemByCartAndInventorySpecification>(), Arg.Any<CancellationToken>())
                .Returns((CartItemEntity?)null);

            var cart = await _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None);

            await _cartRepo.Received(1).AddAsync(Arg.Is<CartEntity>(c => c.CustomerId == customerId), Arg.Any<CancellationToken>());

            await _cartItemRepo.Received(1).AddAsync(
                Arg.Is<CartItemEntity>(i =>
                    i.CartId != Guid.Empty &&
                    i.InventoryItemId == product.Id &&
                    i.Quantity == 1 &&
                    i.ProductName == product.ProductName &&
                    i.Price == product.Price),
                Arg.Any<CancellationToken>());

            await _cartRepo.DidNotReceive().UpdateAsync(Arg.Any<CartEntity>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AddItemForUserAsync_IncrementsQuantity_WhenItemExists()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(10);
            var cart = new CartEntity { Id = Guid.NewGuid(), CustomerId = customerId };

            var existingItem = new CartItemEntity
            {
                CartId = cart.Id,
                InventoryItemId = product.Id,
                Quantity = 1,
                ProductName = "Old",
                Price = 5m
            };

            _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>())
                .Returns(cart);

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(product);

            _cartItemRepo.GetAsync(Arg.Any<GetCartItemByCartAndInventorySpecification>(), Arg.Any<CancellationToken>())
                .Returns(existingItem);

            await _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None);

            await _cartItemRepo.Received(1).UpdateAsync(
                Arg.Is<CartItemEntity>(i =>
                    i.InventoryItemId == product.Id &&
                    i.Quantity == 2 &&
                    i.ProductName == product.ProductName &&
                    i.Price == product.Price &&
                    i.PromotionPrice == null),
                Arg.Any<CancellationToken>());

            await _cartItemRepo.DidNotReceive().AddAsync(Arg.Any<CartItemEntity>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AddItemForUserAsync_Throws_WhenProductNotFound()
        {
            var customerId = Guid.NewGuid();

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns((InventoryItemEntity?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _cartService.AddItemForUserAsync(customerId, Guid.NewGuid(), CancellationToken.None));

            await _cartRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
            await _cartRepo.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
            await _cartItemRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
            await _cartItemRepo.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
        }

        [Fact]
        public async Task AddItemForUserAsync_Throws_WhenOutOfStock()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(0);

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(product);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None));

            await _cartItemRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
            await _cartItemRepo.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
        }

        [Fact]
        public async Task AddItemForUserAsync_Throws_WhenExceedsStock_RED()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(1);
            var cart = new CartEntity { Id = Guid.NewGuid(), CustomerId = customerId };

            var existingItem = new CartItemEntity
            {
                CartId = cart.Id,
                InventoryItemId = product.Id,
                Quantity = 1,
                ProductName = product.ProductName,
                Price = product.Price
            };

            _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>()).Returns(cart);
            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(product);
            _cartItemRepo.GetAsync(Arg.Any<GetCartItemByCartAndInventorySpecification>(), Arg.Any<CancellationToken>())
                .Returns(existingItem);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None));

            await _cartItemRepo.DidNotReceive().UpdateAsync(Arg.Any<CartItemEntity>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AddItemForUserAsync_Updates_LastUpdatedAt()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(5);

            var cart = new CartEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                LastUpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                Items = new List<CartItemEntity>()
            };

            _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>()).Returns(cart);
            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(product);
            _cartItemRepo.GetAsync(Arg.Any<GetCartItemByCartAndInventorySpecification>(), Arg.Any<CancellationToken>())
                .Returns((CartItemEntity?)null);

            var before = cart.LastUpdatedAt;
            var result = await _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None);

            Assert.True(result.LastUpdatedAt > before);
            await _cartItemRepo.Received(1).AddAsync(Arg.Any<CartItemEntity>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AddItemForUserAsync_SetsPromotionPrice_WhenProductIsPromoted()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(5);
            product.IsPromoted = true;
            product.PromotionPrice = 7m;

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(product);

            _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>())
                .Returns(new CartEntity { Id = Guid.NewGuid(), CustomerId = customerId });

            _cartItemRepo.GetAsync(Arg.Any<GetCartItemByCartAndInventorySpecification>(), Arg.Any<CancellationToken>())
                .Returns((CartItemEntity?)null);

            await _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None);

            await _cartItemRepo.Received(1).AddAsync(
                Arg.Is<CartItemEntity>(i =>
                    i.InventoryItemId == product.Id &&
                    i.PromotionPrice == 7m),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AddItemForUserAsync_ClearsPromotionPrice_WhenProductNotPromoted()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(5);
            product.IsPromoted = false;
            product.PromotionPrice = null;

            var cart = new CartEntity { Id = Guid.NewGuid(), CustomerId = customerId };

            var existingItem = new CartItemEntity
            {
                CartId = cart.Id,
                InventoryItemId = product.Id,
                Quantity = 1,
                PromotionPrice = 5m
            };

            _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>()).Returns(cart);
            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(product);
            _cartItemRepo.GetAsync(Arg.Any<GetCartItemByCartAndInventorySpecification>(), Arg.Any<CancellationToken>())
                .Returns(existingItem);

            await _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None);

            await _cartItemRepo.Received(1).UpdateAsync(
                Arg.Is<CartItemEntity>(i =>
                    i.InventoryItemId == product.Id &&
                    i.PromotionPrice == null),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AddItemForUserAsync_AddsDifferentProducts_AsSeparateLines()
        {
            var customerId = Guid.NewGuid();
            var product1 = MakeProduct(5);
            var product2 = MakeProduct(10);

            var cart = new CartEntity { Id = Guid.NewGuid(), CustomerId = customerId };

            _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>()).Returns(cart);

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(product2);

            _cartItemRepo.GetAsync(Arg.Any<GetCartItemByCartAndInventorySpecification>(), Arg.Any<CancellationToken>())
                .Returns((CartItemEntity?)null);

            _ = await _cartService.AddItemForUserAsync(customerId, product2.Id, CancellationToken.None);

            await _cartItemRepo.Received(1).AddAsync(
                Arg.Is<CartItemEntity>(i =>
                    i.InventoryItemId == product2.Id && i.Quantity == 1),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AddItemForUserAsync_ShouldThrow_WhenStatusIsUnavailable()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(3);
            product.Status = InventoryItemStatusEnum.Unavailable;

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(product);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None));
        }

        [Fact]
        public async Task AddItemForUserAsync_ShouldUpdateCartTimestamp()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(2);

            var cart = new CartEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                LastUpdatedAt = DateTime.UtcNow.AddHours(-1),
                Items = new List<CartItemEntity>()
            };

            _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>()).Returns(cart);
            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(product);
            _cartItemRepo.GetAsync(Arg.Any<GetCartItemByCartAndInventorySpecification>(), Arg.Any<CancellationToken>())
                .Returns((CartItemEntity?)null);

            var before = cart.LastUpdatedAt;
            var result = await _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None);

            Assert.True(result.LastUpdatedAt > before);
        }

        [Fact]
        public async Task AddItemForUserAsync_ShouldApplyPromotionPrice_WhenProductIsOnPromotion()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(5);
            product.IsPromoted = true;
            product.PromotionPrice = 4.99m;

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(product);

            _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>())
                .Returns(new CartEntity { Id = Guid.NewGuid(), CustomerId = customerId });

            _cartItemRepo.GetAsync(Arg.Any<GetCartItemByCartAndInventorySpecification>(), Arg.Any<CancellationToken>())
                .Returns((CartItemEntity?)null);

            await _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None);

            await _cartItemRepo.Received(1).AddAsync(
                Arg.Is<CartItemEntity>(i =>
                    i.InventoryItemId == product.Id &&
                    i.PromotionPrice == 4.99m),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task AddItemForUserAsync_ShouldCreateSeparateLine_ForDifferentProducts()
        {
            var customerId = Guid.NewGuid();
            var product1 = MakeProduct(5);
            var product2 = MakeProduct(7);

            var cart = new CartEntity { Id = Guid.NewGuid(), CustomerId = customerId };

            _cartRepo.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>()).Returns(cart);

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(product2);

            _cartItemRepo.GetAsync(Arg.Any<GetCartItemByCartAndInventorySpecification>(), Arg.Any<CancellationToken>())
                .Returns((CartItemEntity?)null);

            _ = await _cartService.AddItemForUserAsync(customerId, product2.Id, CancellationToken.None);

            await _cartItemRepo.Received(1).AddAsync(
                Arg.Is<CartItemEntity>(i => i.InventoryItemId == product2.Id && i.Quantity == 1),
                Arg.Any<CancellationToken>());
        }

        private static InventoryItemEntity MakeProduct(int availableAmount)
        {
            return new InventoryItemEntity
            {
                Id = Guid.NewGuid(),
                ProductUpc = "123456",
                ProductName = "Test",
                Price = 10m,
                Description = "desc",
                CategoryId = 1,
                Category = new CategoryEntity
                {
                    Id = 1,
                    Name = "Cat",
                    URL = "http://example.com",
                    Visibility = true,
                    ShopId = 1
                },
                Status = InventoryItemStatusEnum.Available,
                AvailableAmount = availableAmount,
                ReservedAmount = 0,
                OnHandAmount = availableAmount,
                ProductLocation = "Shelf",
                ShopId = 1,
                BrandId = 1,
                Brand = new BrandEntity
                {
                    Id = 1,
                    Name = "Brand",
                    ShopId = 1
                },
                LastUpdatedDate = DateTime.UtcNow,
                IsActive = true
            };
        }
    }
}

