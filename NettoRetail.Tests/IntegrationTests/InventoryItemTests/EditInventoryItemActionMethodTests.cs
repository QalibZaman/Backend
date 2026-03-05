using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Tests.IntegrationTests.InventoryItemTests.EditInventoryItemActionMethodTests;

[Collection("Edit Inventory Item - Action Method Tests")]
public class EditInventoryItemActionMethodTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _route = "/api/v1/inventory-items/full-info/";
    public EditInventoryItemActionMethodTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

        SeedData();
    }

    [Fact]
    public async Task EditInventoryItem_AccessWithUnauthenticatedUser_Returns401Unauthorized()
    {
        var payload = new UpdateProductModel();
        var irrelevantId = Guid.NewGuid();

        var response = await _client.PatchAsJsonAsync(_route + irrelevantId, payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EditInventoryItem_AccessAsCustomer_Returns403Forbidden()
    {
        var payload = new UpdateProductModel();
        var irrelevantId = Guid.NewGuid();

        using HttpRequestMessage request = await GetAuthenticatedRequest("customer@example.com", "Password123@", _route + irrelevantId, payload);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task EditInventoryItem_AccessAsAdminWithEmptyPayload_ShouldReturn400BadRequest()
    {
        var payload = new UpdateProductModel()
        {
            GeneralInfo = new GeneralInfoModel()
        };
        var itemId = "87abb5e5-7062-4a2f-a3c3-f0e14c095d27";

        using HttpRequestMessage request = await GetAuthenticatedRequest("admin@example.com", "Password123@", _route + itemId, payload);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EditInventoryItem_AccessAsAdmin_Returns204NoContent()
    {
        var payload = new UpdateProductModel()
        {
            GeneralInfo = new GeneralInfoModel()
            {
                Description = "Updated description, so that validation passes."
            }
        };
        var itemId = "87abb5e5-7062-4a2f-a3c3-f0e14c095d27";

        using HttpRequestMessage request = await GetAuthenticatedRequest("admin@example.com", "Password123@", _route + itemId, payload);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task EditInventoryItem_PassingInvalidProductName_Returns400BadRequest()
    {
        var payload = new UpdateProductModel()
        {
            GeneralInfo = new GeneralInfoModel()
            {
                ProductName = ""
            }
        };
        var itemId = "87abb5e5-7062-4a2f-a3c3-f0e14c095d27";

        using HttpRequestMessage request = await GetAuthenticatedRequest("super.admin@example.com", "Password123@", _route + itemId, payload);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EditInventoryItem_PassingValidDescription_Returns204NoContent()
    {
        var payload = new UpdateProductModel()
        {
            GeneralInfo = new GeneralInfoModel()
            {
                Description = "It is a poor sort of memory that only works backwards."
            }
        };
        var itemId = "87abb5e5-7062-4a2f-a3c3-f0e14c095d27";

        using HttpRequestMessage request = await GetAuthenticatedRequest("admin@example.com", "Password123@", _route + itemId, payload);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task EditInventoryItem_PassingValidDescription_UpdatesOnlyDescription()
    {
        var payload = new UpdateProductModel()
        {
            GeneralInfo = new GeneralInfoModel()
            {
                Description = "It is a poor sort of memory that only works backwards."
            }
        };
        var itemId = "87abb5e5-7062-4a2f-a3c3-f0e14c095d27";

        var beforeItem = await GetInventoryItemAsync(itemId);

        using HttpRequestMessage request = await GetAuthenticatedRequest("admin@example.com", "Password123@", _route + itemId, payload);

        var response = await _client.SendAsync(request);

        var afterItem = await GetInventoryItemAsync(itemId);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(afterItem);
        Assert.Equal(payload.GeneralInfo.Description, afterItem.Description);

        Assert.NotNull(beforeItem);
        Assert.Equal(beforeItem.HasVariations, afterItem.HasVariations);
        Assert.Equal(beforeItem.BrandId, afterItem.BrandId);
        Assert.Equal(beforeItem.ProductName, afterItem.ProductName);
        Assert.Equal(beforeItem.CategoryId, afterItem.CategoryId);
        Assert.Equal(beforeItem.ProductUpc, afterItem.ProductUpc);
        Assert.Equal(beforeItem.IsActive, afterItem.IsActive);
    }

    [Fact]
    public async Task EditInventoryItem_PassingValidDescription_UpdatesLastUpdatedDate()
    {
        var payload = new UpdateProductModel()
        {
            GeneralInfo = new GeneralInfoModel()
            {
                Description = "It is a poor sort of memory that only works backwards."
            }
        };
        var itemId = "87abb5e5-7062-4a2f-a3c3-f0e14c095d27";

        var beforeItem = await GetInventoryItemAsync(itemId);
        var timestampBefore = beforeItem!.LastUpdatedDate;

        using HttpRequestMessage request = await GetAuthenticatedRequest("admin@example.com", "Password123@", _route + itemId, payload);

        var response = await _client.SendAsync(request);

        var afterItem = await GetInventoryItemAsync(itemId);
        var timestampAfter = afterItem!.LastUpdatedDate;

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(afterItem);
        Assert.NotNull(beforeItem);
        Assert.True(timestampAfter > timestampBefore);
    }

    [Fact]
    public async Task EditInventoryItem_PassingCorrectPriceInfo_UpdatesCorrectData()
    {
        var payload = new UpdateProductModel()
        {
            PriceInfo = new PriceModel()
            {
                Price = 1000,
                IsPromoted = true,
                StartDate = new DateOnly(2026, 02, 06),
                PromotionPrice = 999.99m
            }
        };
        var itemId = "87abb5e5-7062-4a2f-a3c3-f0e14c095d27";

        var beforeItem = await GetInventoryItemAsync(itemId);
        var timestampBefore = beforeItem!.LastUpdatedDate;

        using HttpRequestMessage request = await GetAuthenticatedRequest("admin@example.com", "Password123@", _route + itemId, payload);

        var response = await _client.SendAsync(request);

        var afterItem = await GetInventoryItemAsync(itemId);
        var timestampAfter = afterItem!.LastUpdatedDate;

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(afterItem);
        Assert.NotNull(beforeItem);
        Assert.True(timestampAfter > timestampBefore);

        Assert.Equal(afterItem.Price, payload.PriceInfo.Price);
        Assert.Equal(afterItem.IsPromoted, payload.PriceInfo.IsPromoted);
        Assert.Equal(afterItem.PromotionPrice, payload.PriceInfo.PromotionPrice);
        Assert.Equal(afterItem.PromotionStartDate, payload.PriceInfo.StartDate);
        Assert.Equal(beforeItem.PromotionEndDate, afterItem.PromotionEndDate);
    }

    [Fact]
    public async Task EditInventoryItem_PassingProductDetailsForDelete_DeletesSpecifiedProductDetail()
    {
        var payload = new UpdateProductModel()
        {
            Details = new List<DetailModel>()
            {
                new DetailModel()
                {
                    Id = new Guid("87abb5e5-6082-4a2f-a3c3-f0e14c095d27"),
                    IsDelete = true
                }
            }
        };
        var itemId = "87abb5e5-7062-4a2f-a3c3-f0e14c095d27";

        var beforeItem = await GetInventoryItemAsync(itemId);
        var timestampBefore = beforeItem!.LastUpdatedDate;

        using HttpRequestMessage request = await GetAuthenticatedRequest("admin@example.com", "Password123@", _route + itemId, payload);

        var response = await _client.SendAsync(request);

        var afterItem = await GetInventoryItemAsync(itemId);
        var timestampAfter = afterItem!.LastUpdatedDate;

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(afterItem);
        Assert.NotNull(beforeItem);
        Assert.True(timestampAfter > timestampBefore);

        Assert.NotEmpty(beforeItem.ProductDetails);
        Assert.Empty(afterItem.ProductDetails);
    }

    [Fact]
    public async Task EditInventoryItem_PassingProductDetailsForBigUpdate_UpdatesProductDetail()
    {
        var newName = "NEW_NAME";
        var payload = new UpdateProductModel()
        {
            Details = new List<DetailModel>()
            {
                new DetailModel()
                {
                    Id = new Guid("87abb5e5-6082-4a2f-a3c3-f0e14c095d27"),
                    Name = newName
                }
            }
        };
        var itemId = "87abb5e5-7062-4a2f-a3c3-f0e14c095d27";

        var beforeItem = await GetInventoryItemAsync(itemId);
        var timestampBefore = beforeItem!.LastUpdatedDate;
        var beforeDetail = beforeItem.ProductDetails.ElementAt(0);

        using HttpRequestMessage request = await GetAuthenticatedRequest("admin@example.com", "Password123@", _route + itemId, payload);

        var response = await _client.SendAsync(request);

        var afterItem = await GetInventoryItemAsync(itemId);
        var timestampAfter = afterItem!.LastUpdatedDate;
        var afterDetail = afterItem.ProductDetails.ElementAt(0);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(afterItem);
        Assert.NotNull(beforeItem);
        Assert.True(timestampAfter > timestampBefore);

        beforeDetail.Should().BeEquivalentTo(
            afterDetail,
            options => options
                .Excluding(af => af.Name)
                .Excluding(af => af.InventoryItem));

        afterDetail.Name.Should().Be(newName);
    }

    [Fact]
    public async Task EditInventoryItem_PassingProductDetailsNameForUpdate_UpdatesProductDetail()
    {
        var newName = "NEW_NAME";
        var payload = new UpdateProductModel()
        {
            Details = new List<DetailModel>()
            {
                new DetailModel()
                {
                    Id = new Guid("87abb5e5-6082-4a2f-a3c3-f0e14c095d27"),
                    Name = newName
                }
            }
        };
        var itemId = "87abb5e5-7062-4a2f-a3c3-f0e14c095d27";

        var beforeItem = await GetInventoryItemAsync(itemId);
        var timestampBefore = beforeItem!.LastUpdatedDate;
        var beforeDetail = beforeItem.ProductDetails.ElementAt(0);

        using HttpRequestMessage request = await GetAuthenticatedRequest("admin@example.com", "Password123@", _route + itemId, payload);

        var response = await _client.SendAsync(request);

        var afterItem = await GetInventoryItemAsync(itemId);
        var timestampAfter = afterItem!.LastUpdatedDate;
        var afterDetail = afterItem.ProductDetails.ElementAt(0);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull(afterItem);
        Assert.NotNull(beforeItem);
        Assert.True(timestampAfter > timestampBefore);

        beforeDetail.Should().BeEquivalentTo(
            afterDetail,
            options => options
                .Excluding(af => af.Name)
                .Excluding(af => af.InventoryItem));

        afterDetail.Name.Should().Be(newName);
    }

    [Fact]
    public async Task EditInventoryItem_TryToUpdateOnlyType_Returns400BadRequestWithExpectedErrorMessages()
    {
        var expectedErrorMessages = new string[]
        {
            "Value shouldn't be null.",
            "Value should be a valid boolean."
        };
        var newType = ProductDetailTypeEnum.Logic;
        const string itemId = "87abb5e5-7062-4a2f-a3c3-f0e14c095d27";
        var payload = new UpdateProductModel()
        {
            Details = new List<DetailModel>()
            {
                new DetailModel()
                {
                    Id = new Guid("87abb5e5-6082-4a2f-a3c3-f0e14c095d27"),
                    TypeEnum = newType
                }
            }
        };

        var beforeItem = await GetInventoryItemAsync(itemId);
        var timestampBefore = beforeItem!.LastUpdatedDate;
        var beforeDetail = beforeItem.ProductDetails.ElementAt(0);

        using HttpRequestMessage request = await GetAuthenticatedRequest("admin@example.com", "Password123@", _route + itemId, payload);

        var response = await _client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        var afterItem = await GetInventoryItemAsync(itemId);
        var timestampAfter = afterItem!.LastUpdatedDate;
        var afterDetail = afterItem.ProductDetails.ElementAt(0);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(timestampBefore, timestampAfter);
        beforeDetail.Should().BeEquivalentTo(
            afterDetail,
            options => options.Excluding((d) => d.InventoryItem)
        );
        responseContent.Should().Contain(expectedErrorMessages[0]);
        responseContent.Should().Contain(expectedErrorMessages[1]);
    }

    [Fact]
    public async Task EditInventoryItem_TryToAddNewDetails_Returns204NoContentAndAddsItems()
    {
        var payload = new UpdateProductModel()
        {
            Details = new List<DetailModel>()
            {
                new DetailModel()
                {
                    Name = "CaseColor",
                    TypeEnum = ProductDetailTypeEnum.Color,
                    Value = "#FFF"
                }
            }
        };

        const string itemId = "87abb5e5-7062-4a2f-a3c3-f0e14c095d27";

        using HttpRequestMessage request = await GetAuthenticatedRequest("admin@example.com", "Password123@", _route + itemId, payload);

        var beforeItem = await GetInventoryItemAsync(itemId);
        var timestampBefore =  beforeItem!.LastUpdatedDate;

        var response = await _client.SendAsync(request);

        var afterItem = await GetInventoryItemAsync(itemId);
        var timestampAfter = afterItem!.LastUpdatedDate;

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(timestampAfter > timestampBefore);
        Assert.Equal(2, afterItem.ProductDetails.Count);
        Assert.Equal(1, beforeItem.ProductDetails.Count);
    }

    private async Task<InventoryItemEntity?> GetInventoryItemAsync(string id)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await dbContext.InventoryItems
            .Where((item) => item.Id.ToString() == id)
            .Include((item) => item.ProductDetails)
            .SingleAsync();
    }

    private async Task<HttpRequestMessage> GetAuthenticatedRequest(string email, string password, string uri, UpdateProductModel payload)
    {
        var tokenResponse = await GetTokenResponseAsync(email, password);
        var request = new HttpRequestMessage(HttpMethod.Patch, uri);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        request.Content = JsonContent.Create(payload);

        return request;
    }

    private async Task<TokenResponse> GetTokenResponseAsync(string email, string password)
    {
        var loginModel = new LoginModel(email, password);
        var response = await _client.PostAsJsonAsync("api/v1/staff/log-in", loginModel);

        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();

        return responseData?.Data ?? throw new InvalidDataException("Token data was null");
    }

    private void SeedData()
    {
        using(var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            var defaultCountry = dbContext.Countries.Single(c => c.Name == "Austria");

            var subscriptions = new List<SubscriptionPlanEntity>()
            {
                new SubscriptionPlanEntity()
                {
                    Id = 1,
                    Type = 0,
                    Description = "Free plan",
                    MonthlyPrice = 0,
                    Moto = "Try the app for free",
                    SubscriptionId = Guid.NewGuid().ToString(),
                    Name = "Free"
                },
                new SubscriptionPlanEntity()
                {
                    Id = 2,
                    Type = 0,
                    Description = "Super plan",
                    MonthlyPrice = 20,
                    Moto = "Super features",
                    SubscriptionId = Guid.NewGuid().ToString(),
                    Name = "Super"
                },
                new SubscriptionPlanEntity()
                {
                    Id = 3,
                    Type = 0,
                    Description = "Business plan",
                    MonthlyPrice = 30,
                    Moto = "Full business power",
                    SubscriptionId = Guid.NewGuid().ToString(),
                    Name = "Business"
                }
            };

            var shops = new List<ShopEntity>()
            {
                new ShopEntity()
                {
                    Id = 1,
                    Name = "Test Shop 1",
                    Email = "test.shop.one@example.com",
                    PhoneNumber = "123 4567890",
                    Url = "test-shop-1",
                    IsActive = true,
                    SubscriptionPlanId = 1,
                    CountryId = 1
                }
            };

            var accounts = new List<StaffEntity>()
            {
                new StaffEntity()
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Admin",
                    LastName = "Admin",
                    Email = "admin@example.com",
                    PasswordHash = PasswordHasher.HashPassword("Password123@"),
                    RoleId = 1,
                    TwoFactorEnabled = false,
                    ShopId = 1
                },
                new StaffEntity()
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Customer",
                    LastName = "Customer",
                    Email = "customer@example.com",
                    PasswordHash = PasswordHasher.HashPassword("Password123@"),
                    RoleId = 2,
                    TwoFactorEnabled = false,
                    ShopId = 1
                },
                new StaffEntity()
                {
                    Id = Guid.NewGuid(),
                    FirstName = "SuperAdmin",
                    LastName = "SuperAdmin",
                    Email = "super.admin@example.com",
                    PasswordHash = PasswordHasher.HashPassword("Password123@"),
                    RoleId = 99,
                    TwoFactorEnabled = false,
                    ShopId = 1
                }
            };

            var categories = new List<CategoryEntity>()
            {
                new CategoryEntity()
                {
                    Id = 1,
                    Name = "Test Category 1",
                    URL = "test-category-1",
                    Visibility = true,
                    Level = 1,
                    ShopId = 1
                }
            };

            var brands = new List<BrandEntity>()
            {
                new BrandEntity()
                {
                    Id = 1,
                    Name = "Test Brand 1",
                    ShopId = 1
                }
            };

            var details = new List<ProductDetailEntity>()
            {
                new ProductDetailEntity()
                {
                    Id = new Guid("87abb5e5-6082-4a2f-a3c3-f0e14c095d27"),
                    Name = "Mass",
                    TypeEnum = ProductDetailTypeEnum.Number,
                    Unit = ProductDetailUnitEnum.kg,
                    Value = "20",
                    InventoryItemId = new Guid("87abb5e5-7062-4a2f-a3c3-f0e14c095d27"),
                    OrderIndex = 1
                }
            };

            var items = new List<InventoryItemEntity>()
            {
                new InventoryItemEntity()
                {
                    Id = new Guid("87abb5e5-7062-4a2f-a3c3-f0e14c095d27"),
                    ProductName = "Test Item 1",
                    ProductUpc = "123456789012",
                    Price = 100.00m,
                    Description = "Test Item 1. Exists for integration tests.",
                    CategoryId = 1,
                    Category = categories[0],
                    LastUpdatedDate = DateTime.UtcNow,
                    IsActive = true,
                    Status = InventoryItemStatusEnum.Available,
                    AvailableAmount = 10,
                    ReservedAmount = 2,
                    OnHandAmount = 8,
                    ProductLocation = "Test Location",
                    ShopId = 1,
                    BrandId = 1,
                    Brand = brands[0],
                    ProductDetails = details
                }
            };

            dbContext.SubscriptionPlans.AddRange(subscriptions);
            dbContext.Shops.AddRange(shops);
            dbContext.Staff.AddRange(accounts);
            dbContext.Categories.AddRange(categories);
            dbContext.Brands.AddRange(brands);
            dbContext.InventoryItems.AddRange(items);
            dbContext.ProductDetails.AddRange(details);

            dbContext.SaveChanges();
        }
    }
}