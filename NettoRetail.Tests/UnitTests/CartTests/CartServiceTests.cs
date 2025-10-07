using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.CartServiceTest
{
    public class CartServiceTests
    {
        private readonly ICartRepository _cartRepo;
        private readonly IInventoryItemRepository _invRepo;
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _cartRepo = Substitute.For<ICartRepository>();
            _invRepo = Substitute.For<IInventoryItemRepository>();
            _cartService = new CartService(_cartRepo, _invRepo);
        }


        [Fact]
        public async Task AddItemForUserAsync_AddsNewItem_WhenCartDoesNotExist()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(5);

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), CancellationToken.None)
                .Returns(product);

            _cartRepo.GetByCustomerIdAsync(customerId, CancellationToken.None)
                .Returns((CartEntity?)null);

            var cart = await _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None);

            Assert.Single(cart.Items);
            Assert.Equal(product.Id, cart.Items[0].InventoryItemId);
            Assert.Equal(1, cart.Items[0].Quantity);

            await _cartRepo.Received(2).GetByCustomerIdAsync(customerId, CancellationToken.None);
            await _cartRepo.Received(1).AddAsync(Arg.Any<CartEntity>(), CancellationToken.None);
            await _cartRepo.DidNotReceive().UpdateAsync(Arg.Any<CartEntity>(), CancellationToken.None);
        }


        [Fact]
        public async Task AddItemForUserAsync_IncrementsQuantity_WhenItemExists()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(10);

            var existingCart = new CartEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Items = new List<CartItemEntity>
                {
                    new CartItemEntity
                    {
                        InventoryItemId = product.Id,
                        ProductName = "Old",
                        Price = 5,
                        Quantity = 1
                    }
                }
            };

            _cartRepo.GetByCustomerIdAsync(customerId, CancellationToken.None).Returns(existingCart);
            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), CancellationToken.None).Returns(product);

            var cart = await _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None);

            Assert.Single(cart.Items);
            Assert.Equal(2, cart.Items[0].Quantity);
            Assert.Equal(product.ProductName, cart.Items[0].ProductName);
            Assert.Equal(product.Price, cart.Items[0].Price);

            await _cartRepo.Received(2).GetByCustomerIdAsync(customerId, CancellationToken.None);
            await _cartRepo.Received(1).UpdateAsync(Arg.Any<CartEntity>(), CancellationToken.None);
        }


        [Fact]
        public async Task AddItemForUserAsync_Throws_WhenProductNotFound()
        {
            var customerId = Guid.NewGuid();

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), CancellationToken.None)
                .Returns((InventoryItemEntity?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _cartService.AddItemForUserAsync(customerId, Guid.NewGuid(), CancellationToken.None));

            await _cartRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
            await _cartRepo.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
        }

        [Fact]
        public async Task AddItemForUserAsync_Throws_WhenOutOfStock()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(0);

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), CancellationToken.None)
                .Returns(product);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None));
        }


        [Fact]
        public async Task AddItemForUserAsync_Throws_WhenExceedsStock_RED()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(1);

            var cart = new CartEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Items = new List<CartItemEntity>
                {
                    new CartItemEntity
                    {
                        InventoryItemId = product.Id,
                        ProductName = product.ProductName,
                        Price = product.Price,
                        Quantity = 1
                    }
                }
            };

            _cartRepo.GetByCustomerIdAsync(customerId, CancellationToken.None).Returns(cart);
            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), CancellationToken.None).Returns(product);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None));
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

            _cartRepo.GetByCustomerIdAsync(customerId, CancellationToken.None).Returns(cart);
            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), CancellationToken.None).Returns(product);

            var before = cart.LastUpdatedAt;

            var result = await _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None);

            Assert.True(result.LastUpdatedAt > before);
        }

        [Fact]
        public async Task AddItemForUserAsync_SetsPromotionPrice_WhenProductIsPromoted()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(5);
            product.IsPromoted = true;
            product.PromotionPrice = 7m;

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), CancellationToken.None).Returns(product);
            _cartRepo.GetByCustomerIdAsync(customerId, CancellationToken.None).Returns((CartEntity?)null);

            var cart = await _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None);

            Assert.Equal(7m, cart.Items[0].PromotionPrice);
        }

        [Fact]
        public async Task AddItemForUserAsync_ClearsPromotionPrice_WhenProductNotPromoted()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(5);
            product.IsPromoted = false;
            product.PromotionPrice = null;

            var existingCart = new CartEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Items = new List<CartItemEntity>
                {
                    new CartItemEntity
                    {
                        InventoryItemId = product.Id,
                        ProductName = product.ProductName,
                        Price = product.Price,
                        Quantity = 1,
                        PromotionPrice = 5m
                    }
                }
            };

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), CancellationToken.None).Returns(product);
            _cartRepo.GetByCustomerIdAsync(customerId, CancellationToken.None).Returns(existingCart);

            var cart = await _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None);

            Assert.Null(cart.Items[0].PromotionPrice);
        }


        [Fact]
        public async Task AddItemForUserAsync_AddsDifferentProducts_AsSeparateLines()
        {
            var customerId = Guid.NewGuid();
            var product1 = MakeProduct(5);
            var product2 = MakeProduct(10);

            var cart = new CartEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Items = new List<CartItemEntity>
                {
                    new CartItemEntity
                    {
                        InventoryItemId = product1.Id,
                        ProductName = product1.ProductName,
                        Price = product1.Price,
                        Quantity = 1
                    }
                }
            };

            _cartRepo.GetByCustomerIdAsync(customerId, CancellationToken.None).Returns(cart);
            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), CancellationToken.None).Returns(product2);

            var result = await _cartService.AddItemForUserAsync(customerId, product2.Id, CancellationToken.None);

            Assert.Equal(2, result.Items.Count);
            Assert.Contains(result.Items, i => i.InventoryItemId == product1.Id);
            Assert.Contains(result.Items, i => i.InventoryItemId == product2.Id);
        }


        [Fact]
        public async Task AddItemForUserAsync_ShouldThrow_WhenStatusIsUnavailable()
        {
            var customerId = Guid.NewGuid();
            var product = MakeProduct(3);
            product.Status = InventoryItemStatusEnum.Unavailable;

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), CancellationToken.None).Returns(product);

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

            _cartRepo.GetByCustomerIdAsync(customerId, CancellationToken.None).Returns(cart);
            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), CancellationToken.None).Returns(product);

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

            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), CancellationToken.None).Returns(product);
            _cartRepo.GetByCustomerIdAsync(customerId, CancellationToken.None).Returns((CartEntity?)null);

            var cart = await _cartService.AddItemForUserAsync(customerId, product.Id, CancellationToken.None);

            Assert.Equal(4.99m, cart.Items[0].PromotionPrice);
        }


        [Fact]
        public async Task AddItemForUserAsync_ShouldCreateSeparateLine_ForDifferentProducts()
        {
            var customerId = Guid.NewGuid();
            var product1 = MakeProduct(5);
            var product2 = MakeProduct(7);

            var existingCart = new CartEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Items = new List<CartItemEntity>
                {
                    new CartItemEntity
                    {
                    InventoryItemId = product1.Id,
                    ProductName = product1.ProductName,
                    Price = product1.Price,
                    Quantity = 1
                    }
                }
            };

            _cartRepo.GetByCustomerIdAsync(customerId, CancellationToken.None).Returns(existingCart);
            _invRepo.GetAsync(Arg.Any<GetInventoryItemByIdSpecification>(), CancellationToken.None).Returns(product2);

            var cart = await _cartService.AddItemForUserAsync(customerId, product2.Id, CancellationToken.None);

            Assert.Equal(2, cart.Items.Count);
            Assert.Contains(cart.Items, i => i.InventoryItemId == product1.Id);
            Assert.Contains(cart.Items, i => i.InventoryItemId == product2.Id);
        }

        private static InventoryItemEntity MakeProduct(int availableAmount)
        {
            return new InventoryItemEntity
            {
                Id = Guid.NewGuid(),
                ProductUpc = 123456,
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
