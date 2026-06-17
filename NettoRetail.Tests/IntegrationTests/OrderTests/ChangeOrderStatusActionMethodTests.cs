using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Request.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;

namespace NettoRetail.Tests.IntegrationTests.OrderTests.ChangeOrderStatusActionMethodTests;

[Collection("Change Order Status - Action Method Tests")]
public class ChangeOrderStatusActionMethodTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _route = "/api/v1/orders/status";

    public ChangeOrderStatusActionMethodTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

        SeedData();
    }

    [Fact]
    public async Task ChangeOrderStatus_AccessWithUnauthenticatedUser_Returns401Unauthorized()
    {
        var requestModel = new UpdateOrderRequest { OrderId = 999, Status = OrderStatusEnum.Processing };
        var uri = BuildQueryString(_route, requestModel);

        var response = await _client.PostAsync(uri, null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangeOrderStatus_AccessAsCustomer_Returns403Forbidden()
    {
        var requestModel = new UpdateOrderRequest { OrderId = 999, Status = OrderStatusEnum.Processing };
        var uri = BuildQueryString(_route, requestModel);

        var login = new LoginModel("stringb@gmail.com", "Stringa1234!");

        var tokenResponse = await GetTokenResponseAsync("/api/v1/customers/log-in/1", login);

        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ChangeOrderStatus_InvalidOrder_Returns404NotFound()
    {
        var requestModel = new UpdateOrderRequest { OrderId = 999, Status = OrderStatusEnum.Processing };
        var uri = BuildQueryString(_route, requestModel);

        var login = new LoginModel("stringa@gmail.com", "Stringa1234!");

        var tokenResponse = await GetTokenResponseAsync("/api/v1/staff/log-in", login);

        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChangeOrderStatus_ValidRequest_Returns200OK()
    {
        var requestModel = new UpdateOrderRequest
            { OrderId = 1, Status = OrderStatusEnum.InProgress, Comment = "Test comment" };
        var uri = BuildQueryString(_route, requestModel);

        var login = new LoginModel("stringa@gmail.com", "Stringa1234!");

        var tokenResponse = await GetTokenResponseAsync("/api/v1/staff/log-in", login);

        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        data.Should().NotBeNull();
        data!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ChangeOrderStatus_ValidRequest_UpdatesStatus()
    {
        var before = await GetOrderAsync(1);

        var requestModel = new UpdateOrderRequest
            { OrderId = 1, Status = OrderStatusEnum.Processing, Comment = "Updated" };
        var uri = BuildQueryString(_route, requestModel);

        var login = new LoginModel("stringa@gmail.com", "Stringa1234!");

        var tokenResponse = await GetTokenResponseAsync("/api/v1/staff/log-in", login);

        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var response = await _client.SendAsync(request);

        var after = await GetOrderAsync(1);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(before);
        Assert.NotNull(after);
        Assert.NotEqual(before.OrderStatus, after.OrderStatus);
    }

    private async Task<OrderEntity?> GetOrderAsync(int id)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Orders
            .Include(o => o.StatusHistory)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    private async Task<TokenResponse> GetTokenResponseAsync(string loginPath, LoginModel login)
    {
        var response = await _client.PostAsJsonAsync(loginPath, login);
        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();
        return responseData?.Data ?? throw new InvalidDataException("Token data was null");
    }

    private void SeedData()
    {
        using var scope = _scopeFactory.CreateScope();
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
                SubscriptionPlanId = 1
            }
        };

        var brands = new List<BrandEntity>
        {
            new BrandEntity
            {
                Id = 1,
                Name = "Apple",
                Description = "Innovative technology products",
                WebsiteUrl = "https://www.apple.com",
                LogoImageUrl = "https://www.apple.com/logo.png",
                ShopId = 1,
                Shop = shops[0]
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
                Shop = shops[0],
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

        var inventoryItems = new List<InventoryItemEntity>
        {
            new InventoryItemEntity
            {
                ProductName = "MacBook-Pro-16",
                ProductUpc = "123456789012",
                Price = 2499.99m,
                Description = "16-inch MacBook Pro with M2 Pro chip",
                CategoryId = 1,
                LastUpdatedDate = DateTime.UtcNow.AddDays(-2),
                IsActive = true,
                Status = InventoryItemStatusEnum.Available,
                AvailableAmount = 15,
                ReservedAmount = 3,
                IncomingAmount = 3,
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
                ProductName = "iPhone-15-Pro",
                ProductUpc = "987654321098",
                Price = 999.00m,
                Description = "Latest iPhone with A16 Bionic chip",
                CategoryId = 2,
                LastUpdatedDate = DateTime.UtcNow.AddDays(-1),
                IsActive = true,
                Status = InventoryItemStatusEnum.Available,
                AvailableAmount = 25,
                ReservedAmount = 5,
                IncomingAmount = 3,
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
                ProductName = "AirPods-Pro-(2nd-Gen)",
                ProductUpc = "567890123456",
                Price = 249.00m,
                Description = "Premium wireless earbuds with ANC",
                CategoryId = 4,
                LastUpdatedDate = DateTime.UtcNow.AddDays(-3),
                IsActive = true,
                Status = InventoryItemStatusEnum.Committed,
                AvailableAmount = 8,
                IncomingAmount = 3,
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
                ProductName = "Samsung-Galaxy-S23-Ultra",
                ProductUpc = "345678901234",
                Price = 1199.99m,
                Description = "Flagship Android smartphone with S Pen",
                CategoryId = 23,
                LastUpdatedDate = DateTime.UtcNow.AddHours(-12),
                IsActive = true,
                Status = InventoryItemStatusEnum.Available,
                AvailableAmount = 18,
                IncomingAmount = 3,
                ReservedAmount = 2,
                OnHandAmount = 20,
                ProductLocation = "Section 1, Row A",
                ShopId = 2,
                Category = categories[9],
                Shop = shops[1],
                Brand = null,
                BrandId = null
            },
            new InventoryItemEntity
            {
                ProductName = "PlayStation-5",
                ProductUpc = "789012345678",
                Price = 499.99m,
                Description = "Next-gen gaming console",
                CategoryId = 19,
                LastUpdatedDate = DateTime.UtcNow.AddDays(-5),
                IsActive = true,
                Status = InventoryItemStatusEnum.Incoming,
                AvailableAmount = 0,
                IncomingAmount = 3,
                ReservedAmount = 0,
                OnHandAmount = 0,
                ProductLocation = "Warehouse",
                ShopId = 2,
                Category = categories[10],
                Shop = shops[1],
                Brand = null,
                BrandId = null
            },
            new InventoryItemEntity
            {
                ProductName = "iPad-Air-(5th-Gen)",
                ProductUpc = "901234567890",
                Price = 599.00m,
                Description = "Powerful tablet with M1 chip",
                CategoryId = 17,
                LastUpdatedDate = DateTime.UtcNow,
                IsActive = false,
                Status = InventoryItemStatusEnum.Unavailable,
                IncomingAmount = 3,
                AvailableAmount = 0,
                ReservedAmount = 0,
                OnHandAmount = 0,
                ProductLocation = "Storage Room",
                ShopId = 2,
                Category = categories[13],
                Shop = shops[1],
                Brand = null,
                BrandId = null
            }
        };
        var customers = new List<CustomerEntity>()
        {
            new CustomerEntity
            {
                Id = Guid.Parse("5cdb181b-fe81-4c82-bd8e-21362fdd5789"),
                FirstName = "Stringb",
                LastName = "Stringb",
                Email = "stringb@gmail.com",
                PasswordHash =
                    "C7EA0B35808B733C17F70F1A63BBC7428B62EB71D13608EF2996292A7685A99E-143B98D2518CF95011C47FD9220C51EA",
                ShopId = 1,
                RoleId = 2,
                Cart = new CartEntity()
            }
        };

        var accounts = new List<StaffEntity>()
        {
            new StaffEntity
            {
                Id = Guid.Parse("5cdb181b-fe81-4c82-bd8e-21362fdd5688"),
                FirstName = "Stringa",
                LastName = "Stringa",
                Email = "stringa@gmail.com",
                PasswordHash =
                    "C7EA0B35808B733C17F70F1A63BBC7428B62EB71D13608EF2996292A7685A99E-143B98D2518CF95011C47FD9220C51EA",
                RoleId = 1,
                TwoFactorEnabled = false,
                EncryptedTotpSecret = null,
                ShopId = 1,
            },

            new StaffEntity
            {
                Id = Guid.Parse("ccb75bc3-13a1-423a-b04b-e5d16953587c"),
                FirstName = "Stringaa",
                LastName = "Stringaa",
                Email = "stringaa@gmail.com",
                PasswordHash =
                    "FDEDE7047AFF0E2FC840F95EF6D8C5255A4D804BD4C51A77C370BA1B13B7C6A5-DA8F3F2275E5EAD6B3DBE9B696937B77",
                RoleId = 1,
                TwoFactorEnabled = false,
                EncryptedTotpSecret = null,
            },
        };
        var orderItem = new OrderItemEntity
        {
            InventoryItemId = inventoryItems[1].Id,
            InventoryItem = inventoryItems[1],
            Quantity = 2
        };

        var statusHistory = new List<OrderStatusHistoryEntity>
        {
            new OrderStatusHistoryEntity
            {
                OrderId = 1,
                Id = Guid.NewGuid(),
                UpdatedAt = DateTime.UtcNow,
                OldStatus = OrderStatusEnum.New,
                NewStatus = OrderStatusEnum.Processing,
                Comment = "Hello it was perfect.",
                UpdatedBy = accounts[1]
            }
        };

        var order = new OrderEntity
        {
            Id = 1,
            CreationDate = DateTime.UtcNow,
            Creator = "Tester",
            OrderStatus = OrderStatusEnum.New,
            CustomerId = accounts[0].Id,
            Customer = customers[0],
            ShopId = 1,
            OrderItems = new List<OrderItemEntity> { orderItem },
            StatusHistory = statusHistory
        };
        dbContext.Brands.AddRange(brands);
        dbContext.Shops.AddRange(shops);
        dbContext.Categories.AddRange(categories);
        dbContext.Staff.AddRange(accounts);
        dbContext.Orders.Add(order);

        dbContext.SaveChanges();
    }

    public void Dispose()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        dbContext.Orders.RemoveRange(dbContext.Orders);
        dbContext.OrderStatusHistories.RemoveRange(dbContext.OrderStatusHistories);

        dbContext.Categories.RemoveRange(dbContext.Categories);
        dbContext.Staff.RemoveRange(dbContext.Staff);
        dbContext.Shops.RemoveRange(dbContext.Shops);
        dbContext.Brands.RemoveRange(dbContext.Brands);
        dbContext.InventoryItems.RemoveRange(dbContext.InventoryItems);
        dbContext.Customers.RemoveRange(dbContext.Customers);

        dbContext.SaveChanges();
    }

    private static string BuildQueryString(string baseUri, UpdateOrderRequest model)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["OrderId"] = model.OrderId.ToString();
        query["Status"] = ((int)model.Status).ToString();
        if (!string.IsNullOrEmpty(model.Comment))
            query["Comment"] = model.Comment;

        return $"{baseUri}?{query}";
    }
}