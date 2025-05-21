using System.Data;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Context;
using NettoRetail.Application.Models.Authentication;
using Newtonsoft.Json;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.API.Controllers.V1;
using NettoRetail.Application.CQRS.InventoryItems.Commands;
using NettoRetail.Application.Models.InventoryItem;


namespace NettoRetail.Tests.IntegrationTests;

[Collection("Sequential Tests")]
public class InventoryItemApiTests : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly IServiceScopeFactory _scopeFactory;

    public InventoryItemApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

        SeedData();
    }

    [Fact]
    public async Task AddInventoryItems_ShouldReturnCreated_HasValidData()
    {
        // Arrange
        var email = "stringa@gmail.com";
        var password = "Stringa1234!";

        // Act
        var tokenResponse = await GetTokenResponseAsync(email, password);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var request = new AddInventoryItemModel
        {
            ProductName = "Newproduct",
            Description = "NewProduct",
            CategoryId = 1,
            Price = 123,
            ProductUpc = 123456789123
        };

        var response = await _client.PostAsJsonAsync("api/v1/inventory-items", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AddInventoryItems_ShouldReturnBadRequest_HasInvalidData()
    {
        // Arrange
        var email = "stringa@gmail.com";
        var password = "Stringa1234!";

        // Act
        var tokenResponse = await GetTokenResponseAsync(email, password);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var request = new AddInventoryItemModel
        {
            ProductName = "New  product",
            Description = "NewProduct",
            CategoryId = 1,
            Price = 123,
            ProductUpc = 123456789123
        };

        var response = await _client.PostAsJsonAsync("api/v1/inventory-items", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetFilteredInventoryItems_ShouldReturnUnauthorized_WhenAccountHasNoShop()
    {
        // Arrange
        var email = "stringaa@gmail.com";
        var password = "Stringaa1234!";


        // Act
        var tokenResponse = await GetTokenResponseAsync(email, password);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var itemsResponse = await _client.GetAsync("api/v1/inventory-items?categoryId=2&MaxPrice=1000.00&MinPrice=10.00" +
            "&SortingOptions[0].Field=ProductName&SortingOptions[0].IsDescending=true&SortingOptions[1].Field=Price&SortingOptions[1].IsDescending=true");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, itemsResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateInventoryItem_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var email = "stringa@gmail.com";
        var password = "Stringa1234!";

        // Act
        var tokenResponse = await GetTokenResponseAsync(email, password);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var request = new AddInventoryItemModel
        {
            ProductName = "Newproduct",
            Description = "NewProduct",
            CategoryId = 1,
            Price = 123,
            ProductUpc = 123456789123
        };

        var response = await _client.PostAsJsonAsync("api/v1/inventory-items", request);
        
        var item = await response.Content.ReadFromJsonAsync<ApiResponse<InventoryItemResponse>>();

        var model = new UpdateInventoryItemModel { CategoryId = 1, Description = "dwdwdw", ProductName = "dwwddw", IsActive = true, ProductUpc = 123456789123 };

        var itemsResponse = await _client.PatchAsJsonAsync($"api/v1/inventory-items/{item.Data.Id}", model);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, itemsResponse.StatusCode);
    }

    [Fact]
    public async Task GetFilteredInventoryItems_ShouldReturnItemsMatchingFilterCriteria()
    {
        // Arrange
        var email = "stringa@gmail.com";
        var password = "Stringa1234!";

        // Act
        var tokenResponse = await GetTokenResponseAsync(email, password);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var requestUri = "api/v1/inventory-items?pageSize=3&pageNumber=0&" +
            "categoryId=1&" +
            "MaxPrice=2500.00&" +
            "MinPrice=200.00&" +
            "MaxAvailableAmount=100&" +
            "MinAvailableAmount=0&" +
            "MaxReservedAmount=100&" +
            "MinReservedAmount=0&" +
            "MaxOnHandAmount=100&" +
            "MinOnHandAmount=0";

        var itemsResponse = await _client.GetAsync(requestUri);

        // Assert
        var responseContent = await itemsResponse.Content.ReadAsStringAsync();
        var paginatedResponse = JsonConvert.DeserializeObject<PaginatedResponse<InventoryItemListResponse>>(responseContent);

        Assert.NotNull(paginatedResponse);

        // Verify that all items match the filtering criteria
        Assert.All(paginatedResponse.Items, item =>
        {
            Assert.True(item.Price >= 200.00m);
            Assert.True(item.Price <= 2500.00m);
        });

        var validCategories = new[] { "Electronics", "Computers", "Laptops", "Desktops", "Mobile", "Smartphones", "Tablets" };
        Assert.All(paginatedResponse.Items, item => Assert.Contains(item.ProductCategory, validCategories));
    }

    [Fact]
    public async Task GetSortedInventoryItems_ShouldReturnItemsSortedByProductNameDescending()
    {
        // Arrange
        var email = "stringaaa@gmail.com";
        var password = "Stringaaa1234!";

        // Act
        var tokenResponse = await GetTokenResponseAsync(email, password);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var requestUri = "api/v1/inventory-items?pageSize=3&pageNumber=0&" +
            "SortingOptions[0].Field=ProductName&" +
            "SortingOptions[0].IsDescending=true";

        var itemsResponse = await _client.GetAsync(requestUri);

        // Assert
        var responseContent = await itemsResponse.Content.ReadAsStringAsync();
        var paginatedResponse = JsonConvert.DeserializeObject<PaginatedResponse<InventoryItemListResponse>>(responseContent);

        Assert.NotNull(paginatedResponse);

        var sortedNames = paginatedResponse.Items.Select(i => i.ProductName).OrderByDescending(n => n).ToList();
        Assert.Equal(sortedNames, paginatedResponse.Items.Select(i => i.ProductName).ToList());
    }

    [Fact]
    public async Task DownloadInventoryItems_ShouldReturnFile_WhenDataIsNotEmpty()
    {
        // Arrange
        var email = "stringaaaa@gmail.com";
        var password = "Stringaaaa1234!";

        // Act
        var tokenResponse = await GetTokenResponseAsync(email, password);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var itemsResponse = await _client.GetAsync("api/v1/inventory-items/csv-file");

        // Assert
        itemsResponse.EnsureSuccessStatusCode();
        Assert.NotNull(itemsResponse);
    }

    [Fact]
    public async Task GetFilteredInventoryItems_ShouldReturnEmpty_WhenNoItemsMatchFilters()
    {
        // Arrange
        var email = "stringaaaa@gmail.com";
        var password = "Stringaaaa1234!";

        // Act
        var tokenResponse = await GetTokenResponseAsync(email, password);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var itemsResponse = await _client.GetAsync("api/v1/inventory-items?categoryId=8&MaxPrice=100.00");

        // Assert
        itemsResponse.EnsureSuccessStatusCode();
        var responseContent = await itemsResponse.Content.ReadAsStringAsync();
        var paginatedResponse = JsonConvert.DeserializeObject<PaginatedResponse<InventoryItemListResponse>>(responseContent);

        Assert.NotNull(paginatedResponse);
        Assert.Empty(paginatedResponse.Items);
    }

    [Fact]
    public async Task GetFilteredInventoryItems_ShouldReturnNoItems_WhenNoFiltersApplied()
    {
        // Arrange
        var email = "stringaaaaa@gmail.com";
        var password = "Stringaaaaa1234!";

        // Act
        var tokenResponse = await GetTokenResponseAsync(email, password);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var itemsResponse = await _client.GetAsync("api/v1/inventory-items");

        // Assert
        itemsResponse.EnsureSuccessStatusCode();
        var responseContent = await itemsResponse.Content.ReadAsStringAsync();
        var paginatedResponse = JsonConvert.DeserializeObject<PaginatedResponse<InventoryItemListResponse>>(responseContent);

        Assert.Equal(0, paginatedResponse?.Items.Count);
        Assert.NotNull(paginatedResponse);
    }

    [Fact]
    public async Task GetInventoryItemByUpc_ShouldReturnItem_WhenUpcIsProvided()
    {
        // Arrange
        var email = "stringaaaa@gmail.com";
        var password = "Stringaaaa1234!";
        var upc = 123456789012; 

        // Act
        var tokenResponse = await GetTokenResponseAsync(email, password);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var queryString = $"api/v1/inventory-items/search?upcOrName={upc}";

        var response = await _client.GetAsync(queryString);

        // Assert
        var responseContent = await response.Content.ReadAsStringAsync();
        var itemsResponse = JsonConvert.DeserializeObject<ApiResponse<List<InventoryItemListResponse>>>(responseContent);

        Assert.NotNull(itemsResponse);
        Assert.Single(itemsResponse.Data); // Only one item should be returned
        Assert.Equal(upc, itemsResponse.Data[0].ProductUpc);
        Assert.Equal("MacBook Pro 16", itemsResponse.Data[0].ProductName);
    }

    [Fact]
    public async Task GetInventoryItemByName_ShouldReturnItem_WhenNameIsProvided()
    {
        // Arrange
        var email = "stringaaaa@gmail.com";
        var password = "Stringaaaa1234!";
        var name = "MacBook Pro 16"; 

        // Act
        var tokenResponse = await GetTokenResponseAsync(email, password);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        var queryString = $"api/v1/inventory-items/search?upcOrName={name}";

        var response = await _client.GetAsync(queryString);

        // Assert
        var responseContent = await response.Content.ReadAsStringAsync();
        var itemsResponse = JsonConvert.DeserializeObject<ApiResponse<List<InventoryItemListResponse>>>(responseContent);

        // Assert
        Assert.NotNull(itemsResponse);
        Assert.Single(itemsResponse.Data); 
        Assert.Equal(name, itemsResponse.Data[0].ProductName);
        Assert.Equal(123456789012, itemsResponse.Data[0].ProductUpc); 
    }

    public void Dispose()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            dbContext.InventoryItems.RemoveRange(dbContext.InventoryItems);
            dbContext.Categories.RemoveRange(dbContext.Categories);
            dbContext.Accounts.RemoveRange(dbContext.Accounts);
            dbContext.Shops.RemoveRange(dbContext.Shops);

            dbContext.SaveChanges();
        }
    }

    private async Task<TokenResponse> GetTokenResponseAsync(string email, string password)
    {
        var loginModel = new LoginModel(email, password);
        var response = await _client.PostAsJsonAsync("api/v1/staff/log-in", loginModel);
        var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();

        if (responseData == null)
            throw new InvalidDataException("Response could not be read");

        return responseData.Data;
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
                    IsActive = true
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
                    Brand = null,
                    BrandId = null
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
                    Brand = null,
                    BrandId = null
                },
                new InventoryItemEntity
                {
                    ProductName = "iPad Air (5th Gen)",
                    ProductUpc = 901234567890,
                    Price = 599.00m,
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
                    Brand = null,
                    BrandId = null
                }
            };

            var accounts = new List<AccountEntity>()
            {
                new AccountEntity
                {
                    Id = Guid.Parse("5cdb181b-fe81-4c82-bd8e-21362fdd5688"),
                    FirstName = "Stringa",
                    LastName = "Stringa",
                    Email = "stringa@gmail.com",
                    PasswordHash = "C7EA0B35808B733C17F70F1A63BBC7428B62EB71D13608EF2996292A7685A99E-143B98D2518CF95011C47FD9220C51EA",
                    RoleId = 1,
                    TwoFactorEnabled = false,
                    EncryptedTotpSecret = null,
                    ShopId = 1,
                },

                new AccountEntity
                {
                    Id = Guid.Parse("ccb75bc3-13a1-423a-b04b-e5d16953587c"),
                    FirstName = "Stringaa",
                    LastName = "Stringaa",
                    Email = "stringaa@gmail.com",
                    PasswordHash = "FDEDE7047AFF0E2FC840F95EF6D8C5255A4D804BD4C51A77C370BA1B13B7C6A5-DA8F3F2275E5EAD6B3DBE9B696937B77",
                    RoleId = 1,
                    TwoFactorEnabled = false,
                    EncryptedTotpSecret = null,
                },

                new AccountEntity
                {
                    Id = Guid.Parse("60e9867e-2f60-401d-acdd-5144e375143b"),
                    FirstName = "Stringaaa",
                    LastName = "Stringaaa",
                    Email = "stringaaa@gmail.com",
                    PasswordHash = "5F67188DBDDB5C3135EB5E05892A3AFFF9AAA3A54F4AA2172889AD499C7B66EF-E487EE84B7F96C5E54855112673E92B2",
                    RoleId = 1,
                    TwoFactorEnabled = false,
                    EncryptedTotpSecret = null,
                    ShopId = 1,
                },

                new AccountEntity
                {
                    Id = Guid.Parse("fea6c13b-0524-4c16-a6ac-81781ba6dde2"),
                    FirstName = "Stringaaaa",
                    LastName = "Stringaaaa",
                    Email = "stringaaaa@gmail.com",
                    PasswordHash = "DF1FFAF7248DC2857820506DCBC16F5D5F852BDDAD65DFC4A3EE1DA398A81950-58D37F5AA44D4C5F0567954A2E17A253",
                    RoleId = 1,
                    TwoFactorEnabled = false,
                    EncryptedTotpSecret = null,
                    ShopId = 1,
                },

                new AccountEntity
                {
                    Id = Guid.Parse("81910d65-35cf-4662-88fa-9c9ade55d3af"),
                    FirstName = "Stringaaaaa",
                    LastName = "Stringaaaaa",
                    Email = "stringaaaaa@gmail.com",
                    PasswordHash = "DED1ECDCC1CE9C00C437697554A3D4461607ACEB8C685F1FFDED1600BC8A9450-F023237BBC5C020709C81B1824E4CED7",
                    RoleId = 1,
                    TwoFactorEnabled = false,
                    EncryptedTotpSecret = null,
                    ShopId = 1,
                },
            };

            dbContext.Accounts.AddRange(accounts);
            dbContext.Shops.AddRange(shops);
            dbContext.InventoryItems.AddRange(inventoryItems);
            dbContext.Categories.AddRange(categories);
            dbContext.Categories.AddRange(categories2);

            dbContext.SaveChanges();
        }
    }
}
