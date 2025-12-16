using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Application.CQRS.Shops.Queries;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Shop;
using NettoRetail.Application.Services.IInventoryService;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Tests.IntegrationTests.ShopTests
{
    [Collection("Sequential Tests")]
    public class ShopFilteredCatalogHandlerTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ShopFilteredCatalogHandlerTests(CustomWebApplicationFactory factory)
        {
            _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

            SeedData();
        }

        [Fact]
        public async Task FilterWithoutParameters_ShouldReturnAllShopItemsWithPagination()
        {
            // Arrange
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

            var repository = new InventoryItemRepository(context);
            var handler = new GetShopFilteredCatalogHandler(repository, inventoryService);

            var query = new GetShopFilteredCatalogQuery(
                new GetFilteredCatalogModel(),
                ShopId: 2,
                PageModel: new PaginationRequest { PageNumber = 1, PageSize = 2 }
            );

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(3, result.TotalAmount); 
        }

        [Fact]
        public async Task FilterByIsPromoted_ShouldReturnItemsWithPromotion()
        {
            // Arrange
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

            var repository = new InventoryItemRepository(context);
            var handler = new GetShopFilteredCatalogHandler(repository, inventoryService);

            var query = new GetShopFilteredCatalogQuery(
                new GetFilteredCatalogModel
                {
                    IsPromoted = true,
                },
                ShopId: 2,
                PageModel: new PaginationRequest { PageNumber = 1, PageSize = 10 }
            );

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Items, item =>
            {
                Assert.True(item.IsOnSale);
            });
        }

        [Fact]
        public async Task FilterByNotPromoted_ShouldReturnItemsWithoutPromotion()
        {
            // Arrange
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

            var repository = new InventoryItemRepository(context);
            var handler = new GetShopFilteredCatalogHandler(repository, inventoryService);

            var query = new GetShopFilteredCatalogQuery(
                new GetFilteredCatalogModel
                {
                    IsPromoted = false,
                },
                ShopId: 2,
                PageModel: new PaginationRequest { PageNumber = 1, PageSize = 10 }
            );

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Items, item =>
            {
                Assert.False(item.IsOnSale);
            });
        }

        [Fact]
        public async Task FilterByMinPrice_ShouldReturnItemsGreaterOrEqualToMinPrice()
        {
            // Arrange
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

            var repository = new InventoryItemRepository(context);
            var handler = new GetShopFilteredCatalogHandler(repository, inventoryService);

            var query = new GetShopFilteredCatalogQuery(
                new GetFilteredCatalogModel
                {
                    MinPrice = 500,
                },
                ShopId: 2,
                PageModel: new PaginationRequest { PageNumber = 1, PageSize = 10 }
            );

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Items, item =>
            {
                Assert.True(item.Price >= 500);
            });
        }

        [Fact]
        public async Task FilterByMaxPrice_ShouldReturnItemsLowerOrEqualToMaxPrice()
        {
            // Arrange
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

            var repository = new InventoryItemRepository(context);
            var handler = new GetShopFilteredCatalogHandler(repository, inventoryService);

            var query = new GetShopFilteredCatalogQuery(
                new GetFilteredCatalogModel
                {
                    MaxPrice = 500,
                },
                ShopId: 2,
                PageModel: new PaginationRequest { PageNumber = 1, PageSize = 10 }
            );

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Items, item =>
            {
                Assert.True(item.Price <= 500);
            });
        }

        [Fact]
        public async Task FilterByPrice_ShouldCheckPromotionPrice_WhenItemIsPromoted()
        {
            // Arrange
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

            var repository = new InventoryItemRepository(context);
            var handler = new GetShopFilteredCatalogHandler(repository, inventoryService);

            var query = new GetShopFilteredCatalogQuery(
                new GetFilteredCatalogModel
                {
                    MinPrice = 400,
                    MaxPrice = 550,
                },
                ShopId: 2,
                PageModel: new PaginationRequest { PageNumber = 1, PageSize = 10 }
            );

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.All(result.Items, item =>
            {
                Assert.True((item.PromotionPrice >= 400 && item.PromotionPrice <= 550) || (item.Price >= 400 && item.Price <= 550));
            });
        }

        [Fact]
        public async Task FilterByName_ShouldReturnItemsWithThatName()
        {
            // Arrange
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

            var repository = new InventoryItemRepository(context);
            var handler = new GetShopFilteredCatalogHandler(repository, inventoryService);

            var query = new GetShopFilteredCatalogQuery(
                new GetFilteredCatalogModel
                {
                    ProductName = "amsung",
                },
                ShopId: 2,
                PageModel: new PaginationRequest { PageNumber = 1, PageSize = 10 }
            );

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Items, item =>
            {
                Assert.True(item.ProductName.Contains("amsung", StringComparison.OrdinalIgnoreCase));
            });
        }

        [Fact]
        public async Task FilterByBrands_ShouldReturnItemsWithTheSameBrands()
        {
            // Arrange
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

            var repository = new InventoryItemRepository(context);
            var handler = new GetShopFilteredCatalogHandler(repository, inventoryService);

            var query = new GetShopFilteredCatalogQuery(
                new GetFilteredCatalogModel
                {
                    BrandsNames = new List<string> { "amsung", "App" },
                },
                ShopId: 2,
                PageModel: new PaginationRequest { PageNumber = 1, PageSize = 10 }
            );

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Items, item =>
            {
                Assert.Contains(query.FilteredCatalogModel.BrandsNames, b => item.BrandName.Contains(b, StringComparison.OrdinalIgnoreCase));
            });
        }

        [Fact]
        public async Task FilterByStatus_ShouldReturnItemsWithTheSameStatus()
        {
            // Arrange
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

            var repository = new InventoryItemRepository(context);
            var handler = new GetShopFilteredCatalogHandler(repository, inventoryService);

            var query = new GetShopFilteredCatalogQuery(
                new GetFilteredCatalogModel
                {
                    ItemStatus = "Available",
                },
                ShopId: 2,
                PageModel: new PaginationRequest { PageNumber = 1, PageSize = 10 }
            );

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Items, item =>
            {
                Assert.Equal("Available", item.Status);
            });
        }

        public void Dispose()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.Brands.RemoveRange(dbContext.Brands);
                dbContext.InventoryItems.RemoveRange(dbContext.InventoryItems);
                dbContext.Categories.RemoveRange(dbContext.Categories);
                dbContext.Accounts.RemoveRange(dbContext.Accounts);
                dbContext.Shops.RemoveRange(dbContext.Shops);

                dbContext.SaveChanges();
            }
        }

        private void SeedData()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var countries = dbContext.Countries.Where(c => c.Id == 1 || c.Id == 2).ToList();

                var shops = new List<ShopEntity>
                {
                    new ShopEntity
                    {
                        Id = 1,
                        Name = "Main Street Electronics",
                        Email = "info@mainstreetelectronics.com",
                        PhoneNumber = "123 456789",
                        Url = "www.mainstreetelectronics.com",
                        Address = "123 Main Street",
                        City = "New York",
                        State = "NY",
                        ZipCode = "1234",
                        Description = "Your one-stop shop for all electronics needs",
                        CountryId = 1,
                        Country = countries[0],
                        IsActive = true,
                        StripeCustomerId = "cus_test_1",
                        StripeSubscriptionId = "sub_test_1",
                        SubscriptionPlanId = 1
                    },
                    new ShopEntity
                    {
                        Id = 2,
                        Name = "Tech Haven",
                        Email = "support@techhaven.com",
                        PhoneNumber = "123 45 67",
                        Url = "www.techhaven.com",
                        Address = "456 Tech Boulevard",
                        City = "San Francisco",
                        State = "CA",
                        ZipCode = "2345",
                        Description = "Premium technology and gadgets",
                        CountryId = 1,
                        Country = countries[1],
                        IsActive = true,
                        StripeCustomerId = "cus_test_2",
                        StripeSubscriptionId = "sub_test_2",
                        SubscriptionPlanId = 2
                    }
                };

                var categories = new List<CategoryEntity>
                {
                    // First category tree
                    new CategoryEntity
                    {
                        Id = 1,
                        Name = "Electronics",
                        URL = "electronics",
                        Visibility = true,
                        Level = 1,
                        ParentId = null,
                        ShopId = 1,
                        Shop = shops[0]
                    },
                    new CategoryEntity
                    {
                        Id = 2,
                        Name = "Computers",
                        URL = "electronics/computers",
                        Visibility = true,
                        Level = 2,
                        ParentId = 1,
                        ShopId = 1,
                        Shop = shops[0]
                    },
                    new CategoryEntity
                    {
                        Id = 3,
                        Name = "Laptops",
                        URL = "electronics/computers/laptops",
                        Visibility = true,
                        Level = 3,
                        ParentId = 2,
                        ShopId = 1,
                        Shop = shops[0]
                    },
                    new CategoryEntity
                    {
                        Id = 4,
                        Name = "Desktops",
                        URL = "electronics/computers/desktops",
                        Visibility = true,
                        Level = 3,
                        ParentId = 2,
                        ShopId = 1,
                        Shop = shops[0]
                    },
                    new CategoryEntity
                    {
                        Id = 5,
                        Name = "Mobile",
                        URL = "electronics/mobile",
                        Visibility = true,
                        Level = 2,
                        ParentId = 1,
                        ShopId = 1,
                        Shop = shops[0]
                    },
                    new CategoryEntity
                    {
                        Id = 6,
                        Name = "Smartphones",
                        URL = "electronics/mobile/smartphones",
                        Visibility = true,
                        Level = 3,
                        ParentId = 5,
                        ShopId = 1,
                        Shop = shops[0]
                    },
                    new CategoryEntity
                    {
                        Id = 7,
                        Name = "Tablets",
                        URL = "electronics/mobile/tablets",
                        Visibility = true,
                        Level = 3,
                        ParentId = 5,
                        ShopId = 1,
                        Shop = shops[0]
                    },

                    // Second category tree
                    new CategoryEntity
                    {
                        Id = 8,
                        Name = "Home & Garden",
                        URL = "home-garden",
                        Visibility = true,
                        Level = 1,
                        ParentId = null,
                        ShopId = 1,
                        Shop = shops[0]
                    },
                    new CategoryEntity
                    {
                        Id = 9,
                        Name = "Furniture",
                        URL = "home-garden/furniture",
                        Visibility = true,
                        Level = 2,
                        ParentId = 8,
                        ShopId = 1,
                        Shop = shops[0]
                    },
                    new CategoryEntity
                    {
                        Id = 10,
                        Name = "Living Room",
                        URL = "home-garden/furniture/living-room",
                        Visibility = true,
                        Level = 3,
                        ParentId = 9,
                        ShopId = 1,
                        Shop = shops[0]
                    },
                    new CategoryEntity
                    {
                        Id = 11,
                        Name = "Bedroom",
                        URL = "home-garden/furniture/bedroom",
                        Visibility = true,
                        Level = 3,
                        ParentId = 9,
                        ShopId = 1,
                        Shop = shops[0]
                    },
                    new CategoryEntity
                    {
                        Id = 12,
                        Name = "Outdoor",
                        URL = "home-garden/outdoor",
                        Visibility = true,
                        Level = 2,
                        ParentId = 8,
                        ShopId = 1,
                        Shop = shops[0]
                    },
                    new CategoryEntity
                    {
                        Id = 13,
                        Name = "Patio Furniture",
                        URL = "home-garden/outdoor/patio-furniture",
                        Visibility = true,
                        Level = 3,
                        ParentId = 12,
                        ShopId = 1,
                        Shop = shops[0]
                    },
                    new CategoryEntity
                    {
                        Id = 14,
                        Name = "Grills",
                        URL = "home-garden/outdoor/grills",
                        Visibility = true,
                        Level = 3,
                        ParentId = 12,
                        ShopId = 1,
                        Shop = shops[0]
                    }
                };

                var categories2 = new List<CategoryEntity>()
                { 
                    // First category tree
                    new CategoryEntity
                    {
                        Id = 15,
                        Name = "Electronics",
                        URL = "electronics",
                        Visibility = true,
                        Level = 1,
                        ParentId = null,
                        ShopId = 2,
                        Shop = shops[1]
                    },
                    new CategoryEntity
                    {
                        Id = 16,
                        Name = "Computers",
                        URL = "electronics/computers",
                        Visibility = true,
                        Level = 2,
                        ParentId = 15,
                        ShopId = 2,
                        Shop = shops[1]
                    },
                    new CategoryEntity
                    {
                        Id = 17,
                        Name = "Laptops",
                        URL = "electronics/computers/laptops",
                        Visibility = true,
                        Level = 3,
                        ParentId = 16,
                        ShopId = 2,
                        Shop = shops[1]
                    },
                    new CategoryEntity
                    {
                        Id = 18,
                        Name = "Desktops",
                        URL = "electronics/computers/desktops",
                        Visibility = true,
                        Level = 3,
                        ParentId = 16,
                        ShopId = 2,
                        Shop = shops[1]
                    },
                    new CategoryEntity
                    {
                        Id = 19,
                        Name = "Mobile",
                        URL = "electronics/mobile",
                        Visibility = true,
                        Level = 2,
                        ParentId = 15,
                        ShopId = 2,
                        Shop = shops[1]
                    },
                    new CategoryEntity
                    {
                        Id = 20,
                        Name = "Smartphones",
                        URL = "electronics/mobile/smartphones",
                        Visibility = true,
                        Level = 3,
                        ParentId = 19,
                        ShopId = 2,
                        Shop = shops[1]
                    },
                    new CategoryEntity
                    {
                        Id = 21,
                        Name = "Tablets",
                        URL = "electronics/mobile/tablets",
                        Visibility = true,
                        Level = 3,
                        ParentId = 19,
                        ShopId = 2,
                        Shop = shops[1]
                    },

                    // Second category tree
                    new CategoryEntity
                    {
                        Id = 22,
                        Name = "Home & Garden",
                        URL = "home-garden",
                        Visibility = true,
                        Level = 1,
                        ParentId = null,
                        ShopId = 2,
                        Shop = shops[1]
                    },
                    new CategoryEntity
                    {
                        Id = 23,
                        Name = "Furniture",
                        URL = "home-garden/furniture",
                        Visibility = true,
                        Level = 2,
                        ParentId = 22,
                        ShopId = 2,
                        Shop = shops[1]
                    },
                    new CategoryEntity
                    {
                        Id = 24,
                        Name = "Living Room",
                        URL = "home-garden/furniture/living-room",
                        Visibility = true,
                        Level = 3,
                        ParentId = 23,
                        ShopId = 2,
                        Shop = shops[1]
                    },
                    new CategoryEntity
                    {
                        Id = 25,
                        Name = "Bedroom",
                        URL = "home-garden/furniture/bedroom",
                        Visibility = true,
                        Level = 3,
                        ParentId = 23,
                        ShopId = 2,
                        Shop = shops[1]
                    },
                    new CategoryEntity
                    {
                        Id = 26,
                        Name = "Outdoor",
                        URL = "home-garden/outdoor",
                        Visibility = true,
                        Level = 2,
                        ParentId = 22,
                        ShopId = 2,
                        Shop = shops[1]
                    },
                    new CategoryEntity
                    {
                        Id = 27,
                        Name = "Patio Furniture",
                        URL = "home-garden/outdoor/patio-furniture",
                        Visibility = true,
                        Level = 3,
                        ParentId = 26,
                        ShopId = 2,
                        Shop = shops[1]
                    },
                    new CategoryEntity
                    {
                        Id = 28,
                        Name = "Grills",
                        URL = "home-garden/outdoor/grills",
                        Visibility = true,
                        Level = 3,
                        ParentId = 26,
                        ShopId = 2,
                        Shop = shops[1]
                    }
                };

                var brandItems = new List<BrandEntity>
                {
                    new BrandEntity
                    {
                        Id = 1,
                        Name = "Apple",
                        Description = "Innovative technology products",
                        ShopId = 2,
                        Shop = shops[1]
                    },
                    new BrandEntity
                    {
                        Id = 2,
                        Name = "Samsung",
                        Description = "Cutting-edge electronics and appliances",
                        ShopId = 2,
                        Shop = shops[1]
                    },
                    new BrandEntity
                    {
                        Id = 3,
                        Name = "Sony",
                        Description = "Sony technologies",
                        ShopId = 2,
                        Shop = shops[1]
                    }
                };

                var inventoryItems = new List<InventoryItemEntity>
                {
                    new InventoryItemEntity
                    {
                        ProductName = "MacBook Pro 16",
                        ProductUpc = 123456789012,
                        Price = 2499.99m,
                        Description = "16-inch MacBook Pro with M2 Pro chip",
                        CategoryId = 1,
                        LastUpdatedDate = DateTime.UtcNow.AddDays(-2),
                        IsActive = true,
                        Status = InventoryItemStatusEnum.Available,
                        AvailableAmount = 15,
                        ReservedAmount = 3,
                        OnHandAmount = 18,
                        ProductLocation = "Aisle 3, Shelf B",
                        ShopId = 1,
                        Category = categories[2],
                        Shop = shops[0],
                        Brand = null,
                        BrandId = null
                    },
                    new InventoryItemEntity
                    {
                        ProductName = "iPhone 15 Pro",
                        ProductUpc = 987654321098,
                        Price = 999.00m,
                        Description = "Latest iPhone with A16 Bionic chip",
                        CategoryId = 2,
                        LastUpdatedDate = DateTime.UtcNow.AddDays(-1),
                        IsActive = true,
                        Status = InventoryItemStatusEnum.Available,
                        AvailableAmount = 25,
                        ReservedAmount = 5,
                        OnHandAmount = 30,
                        ProductLocation = "Aisle 1, Shelf A",
                        ShopId = 1,
                        Category = categories[5],
                        Shop = shops[0],
                        Brand = null,
                        BrandId = null
                    },
                    new InventoryItemEntity
                    {
                        ProductName = "AirPods Pro (2nd Gen)",
                        ProductUpc = 567890123456,
                        Price = 249.00m,
                        Description = "Premium wireless earbuds with ANC",
                        CategoryId = 4,
                        LastUpdatedDate = DateTime.UtcNow.AddDays(-3),
                        IsActive = true,
                        Status = InventoryItemStatusEnum.Committed,
                        AvailableAmount = 8,
                        ReservedAmount = 12,
                        OnHandAmount = 20,
                        ProductLocation = "Aisle 2, Shelf C",
                        ShopId = 1,
                        Category = categories[6],
                        Shop = shops[0],
                        Brand = null,
                        BrandId = null
                    },

                    // Shop 2 items
                    new InventoryItemEntity
                    {
                        ProductName = "Samsung Galaxy S23 Ultra",
                        ProductUpc = 345678901234,
                        Price = 1199.99m,
                        PromotionPrice = 799.99m,
                        IsPromoted = true,
                        Description = "Flagship Android smartphone with S Pen",
                        CategoryId = 23,
                        LastUpdatedDate = DateTime.UtcNow.AddHours(-12),
                        IsActive = true,
                        Status = InventoryItemStatusEnum.Available,
                        AvailableAmount = 18,
                        ReservedAmount = 2,
                        OnHandAmount = 20,
                        ProductLocation = "Section 1, Row A",
                        ShopId = 2,
                        Category = categories2[9],
                        Shop = shops[1],
                        Brand = brandItems[1],
                        BrandId = 2
                    },
                    new InventoryItemEntity
                    {
                        ProductName = "PlayStation 5",
                        ProductUpc = 789012345678,
                        Price = 499.99m,
                        Description = "Next-gen gaming console",
                        CategoryId = 19,
                        LastUpdatedDate = DateTime.UtcNow.AddDays(-5),
                        IsActive = true,
                        Status = InventoryItemStatusEnum.Incoming,
                        AvailableAmount = 0,
                        ReservedAmount = 0,
                        OnHandAmount = 0,
                        ProductLocation = "Warehouse",
                        ShopId = 2,
                        Category = categories2[10],
                        Shop = shops[1],
                        Brand = brandItems[2],
                        BrandId = 3
                    },
                    new InventoryItemEntity
                    {
                        ProductName = "iPad Air (5th Gen)",
                        ProductUpc = 901234567890,
                        Price = 599.00m,
                        PromotionPrice = 549.00m,
                        IsPromoted = true,
                        Description = "Powerful tablet with M1 chip",
                        CategoryId = 17,
                        LastUpdatedDate = DateTime.UtcNow,
                        IsActive = false,
                        Status = InventoryItemStatusEnum.Unavailable,
                        AvailableAmount = 0,
                        ReservedAmount = 0,
                        OnHandAmount = 0,
                        ProductLocation = "Storage Room",
                        ShopId = 2,
                        Category = categories2[13],
                        Shop = shops[1],
                        Brand = brandItems[0],
                        BrandId = 1
                    }
                };

                dbContext.Shops.AddRange(shops);
                dbContext.Brands.AddRange(brandItems);
                dbContext.InventoryItems.AddRange(inventoryItems);
                dbContext.Categories.AddRange(categories);
                dbContext.Categories.AddRange(categories2);

                dbContext.SaveChanges();
            }
        }
    }
}
