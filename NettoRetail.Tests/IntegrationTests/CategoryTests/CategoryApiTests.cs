using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Request.Category;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NettoRetail.Shared.Options;

namespace NettoRetail.Tests.IntegrationTests.CategoryTests
{
    [Collection("Sequential Tests")]
    public class CategoryApiTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IServiceScopeFactory _scopeFactory;

        public CategoryApiTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

            SeedData();
        }

        [Fact]
        public async Task GetCategories_ShouldReturnSuccess_HasValidData() 
        {
            // Arrange
            var email = "stringa@gmail.com";
            var password = "Stringa1234!";

            // Act
            var tokenResponse = await GetTokenResponseAsync(email, password);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var itemsResponse = await _client.GetAsync("api/v1/categories?pageNumber=1&pageSize=5&shopId=1");

            // Asserts
            Assert.Equal(HttpStatusCode.OK, itemsResponse.StatusCode);
            Assert.NotNull(itemsResponse);
        }

        [Fact]
        public async Task GetCategories_ShouldReturnBadRequest_HasInvalidData()
        {
            // Arrange
            var email = "stringa@gmail.com";
            var password = "Stringa1234!";

            // Act
            var tokenResponse = await GetTokenResponseAsync(email, password);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var itemsResponse = await _client.GetAsync("api/v1/categories?pageNumber=-12&pageSize=-5&shopId=1");

            // Asserts
            Assert.Equal(HttpStatusCode.BadRequest, itemsResponse.StatusCode);
        }

        [Fact]
        public async Task AddCategory_ShouldReturnCreated_HasValidData()
        {
            // Arrange
            var email = "stringa@gmail.com";
            var password = "Stringa1234!";

            // Act
            var tokenResponse = await GetTokenResponseAsync(email, password);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse. AccessToken);

            var model = new AddCategoryRequest
            {
                Name = "Outdoorz",
                URL = "outdoorz",
                ParentId = null,
                Visibility = true,
            };

            var itemsResponse = await _client.PostAsJsonAsync("api/v1/categories", model);

            // Asserts
            Assert.Equal(HttpStatusCode.Created, itemsResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_ShouldReturnNoContent_HasNoChild()
        {
            // Arrange
            var email = "stringa@gmail.com";
            var password = "Stringa1234!";

            // Act
            var tokenResponse = await GetTokenResponseAsync(email, password);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var itemsResponse = await _client.DeleteAsync("api/v1/categories/6");

            // Asserts
            Assert.Equal(HttpStatusCode.NoContent, itemsResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_ShouldReturnBadRequest_WhenCategoryHasChildren()
        {
            // Arrange
            var email = "stringa@gmail.com";
            var password = "Stringa1234!";

            // Act
            var tokenResponse = await GetTokenResponseAsync(email, password);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var itemsResponse = await _client.DeleteAsync("api/v1/categories/5");

            // Asserts
            Assert.Equal(HttpStatusCode.BadRequest, itemsResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_ShouldReturnNotFound_HasInvalidData()
        {
            // Arrange
            var email = "stringa@gmail.com";
            var password = "Stringa1234!";

            // Act
            var tokenResponse = await GetTokenResponseAsync(email, password);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var itemsResponse = await _client.DeleteAsync("api/v1/categories/123");

            // Asserts
            Assert.Equal(HttpStatusCode.NotFound, itemsResponse.StatusCode);
        }

        [Fact]
        public async Task UpdateCategory_ShouldReturnOK_HasValidData()
        {
            // Arrange
            var email = "stringa@gmail.com";
            var password = "Stringa1234!";

            // Act
            var tokenResponse = await GetTokenResponseAsync(email, password);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var model = new UpdateCategoryRequest
            {
                Name = "Name",
                URL = "name",
                ParentId = null,
                Visibility = true
            };
            var itemsResponse = await _client.PatchAsJsonAsync("api/v1/categories/5", model );

            // Asserts
            Assert.Equal(HttpStatusCode.OK, itemsResponse.StatusCode);
        }

        [Fact]
        public async Task UpdateCategory_ShouldReturnNotFound_HasInvalidData()
        {
            // Arrange
            var email = "stringaa@gmail.com"; //not allowed to edit first shopid category
            var password = "Stringa1234!";

            // Act
            var tokenResponse = await GetTokenResponseAsync(email, password);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var model = new UpdateCategoryRequest
            {
                Name = "Name",
                URL = "name",
                ParentId = null,
                Visibility = true
            };
            var itemsResponse = await _client.PatchAsJsonAsync("api/v1/categories/1", model);

            // Asserts
            Assert.Equal(HttpStatusCode.NotFound, itemsResponse.StatusCode);
        }

        [Fact]
        public async Task GetCategoriesSection_ShouldReturnOk_WithDefaultSelectionIfNoSavedSelection()
        {
            var email = "stringa@gmail.com";
            var password = "Stringa1234!";

            var tokenResponse = await GetTokenResponseAsync(email, password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var rootCategories = new[] { "Appliances", "Beauty", "Computers", "Fashion", "Garden", "Home", "Office" };
            foreach (var categoryName in rootCategories)
            {
                var addCategoryRequest = new AddCategoryRequest
                {
                    Name = categoryName,
                    URL = categoryName.ToLowerInvariant(),
                    ParentId = null,
                    Visibility = true
                };
                var createResponse = await _client.PostAsJsonAsync("api/v1/categories", addCategoryRequest);
                Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            }

            var sectionResponse = await _client.GetAsync("api/v1/categories/section");
            Assert.Equal(HttpStatusCode.OK, sectionResponse.StatusCode);

            var result = await sectionResponse.Content.ReadFromJsonAsync<ApiResponse<GetCategoriesSectionResponse>>();
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(CategoriesSectionOptions.DefaultSelectionCount, result.Data.SelectedCategories.Count);
            Assert.Equal("Appliances", result.Data.SelectedCategories[0].Name);
            Assert.Equal("Beauty", result.Data.SelectedCategories[1].Name);
            Assert.Equal("Computers", result.Data.SelectedCategories[2].Name);
            Assert.Equal("Fashion", result.Data.SelectedCategories[3].Name);
            Assert.Equal("Garden", result.Data.SelectedCategories[4].Name);
            Assert.Equal("Home", result.Data.SelectedCategories[5].Name);
        }

        [Fact]
        public async Task GetCategoriesSection_ShouldReturnOk_ForShopWithNoCategories()
        {
            var email = "stringb@gmail.com";
            var password = "Stringa1234!";

            var tokenResponse = await GetTokenResponseAsync(email, password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var sectionResponse = await _client.GetAsync("api/v1/categories/section");
            Assert.Equal(HttpStatusCode.OK, sectionResponse.StatusCode);

            var result = await sectionResponse.Content.ReadFromJsonAsync<ApiResponse<GetCategoriesSectionResponse>>();
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.SelectedCategories);
            Assert.Equal(0, result.Data.AvailableCategoriesCount);
        }

        [Fact]
        public async Task UpdateCategoriesSection_ShouldReturnNoContent_AndSaveOrder()
        {
            var email = "stringa@gmail.com";
            var password = "Stringa1234!";

            var tokenResponse = await GetTokenResponseAsync(email, password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var request = new EditCategoriesSectionRequest
            {
                Name = "Featured",
                Visibility = true,
                CategoryIds = new List<int> { 8 }
            };

            var response = await _client.PatchAsJsonAsync("api/v1/categories/section", request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var sectionResponse = await _client.GetAsync("api/v1/categories/section");
            Assert.Equal(HttpStatusCode.OK, sectionResponse.StatusCode);
            var section = await sectionResponse.Content.ReadFromJsonAsync<ApiResponse<GetCategoriesSectionResponse>>();
            Assert.NotNull(section);
            Assert.Single(section.Data.SelectedCategories);
            Assert.Equal(8, section.Data.SelectedCategories[0].Id);
        }

        [Fact]
        public async Task UpdateCategoriesSection_ShouldReturnBadRequest_WhenDuplicateCategoryIds()
        {
            var email = "stringa@gmail.com";
            var password = "Stringa1234!";

            var tokenResponse = await GetTokenResponseAsync(email, password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var request = new EditCategoriesSectionRequest
            {
                Name = "Featured",
                Visibility = true,
                CategoryIds = new List<int> { 8, 8 }
            };

            var response = await _client.PatchAsJsonAsync("api/v1/categories/section", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCategoriesSection_ShouldReturnBadRequest_WhenMoreThan8Categories()
        {
            var email = "stringa@gmail.com";
            var password = "Stringa1234!";

            var tokenResponse = await GetTokenResponseAsync(email, password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var catNames = new[] { "ExtraA", "ExtraB", "ExtraC", "ExtraD", "ExtraE", "ExtraF", "ExtraG", "ExtraH", "ExtraI" };
            foreach (var name in catNames)
            {
                var addCategoryRequest = new AddCategoryRequest
                {
                    Name = name,
                    URL = name.ToLowerInvariant(),
                    ParentId = null,
                    Visibility = true
                };
                var createResponse = await _client.PostAsJsonAsync("api/v1/categories", addCategoryRequest);
                Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            }

            var allCategoriesResponse = await _client.GetAsync("api/v1/categories?pageNumber=1&pageSize=20&shopId=1");
            Assert.Equal(HttpStatusCode.OK, allCategoriesResponse.StatusCode);
            var responseData = await allCategoriesResponse.Content.ReadFromJsonAsync<ApiResponse<GetCategoriesResponse>>();
            Assert.NotNull(responseData);
            var categoryIds = responseData.Data.Categories.Select(c => c.Id).OrderBy(id => id).Take(9).ToList();

            var request = new EditCategoriesSectionRequest
            {
                Name = "Too many",
                Visibility = true,
                CategoryIds = categoryIds.Take(CategoriesSectionOptions.MaxSelectionCount + 1).ToList()
            };

            var response = await _client.PatchAsJsonAsync("api/v1/categories/section", request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SearchCategoriesForSection_ShouldReturnFilteredData_WithExcludeIds()
        {
            var email = "stringa@gmail.com";
            var password = "Stringa1234!";

            var tokenResponse = await GetTokenResponseAsync(email, password);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            // Ensure there are visible root categories available.
            var addCategoryA = new AddCategoryRequest { Name = "Kitchen", URL = "kitchen", ParentId = null, Visibility = true };
            var addCategoryB = new AddCategoryRequest { Name = "Kids", URL = "kids", ParentId = null, Visibility = true };
            var addCategoryC = new AddCategoryRequest { Name = "Kites", URL = "kites", ParentId = null, Visibility = true };

            var createA = await _client.PostAsJsonAsync("api/v1/categories", addCategoryA);
            var createB = await _client.PostAsJsonAsync("api/v1/categories", addCategoryB);
            var createC = await _client.PostAsJsonAsync("api/v1/categories", addCategoryC);
            Assert.Equal(HttpStatusCode.Created, createA.StatusCode);
            Assert.Equal(HttpStatusCode.Created, createB.StatusCode);
            Assert.Equal(HttpStatusCode.Created, createC.StatusCode);

            var getResponse = await _client.GetAsync("api/v1/categories?pageNumber=1&pageSize=20&shopId=1");
            var allData = await getResponse.Content.ReadFromJsonAsync<ApiResponse<GetCategoriesResponse>>();
            Assert.NotNull(allData);
            Assert.NotNull(allData.Data);
            var allIds = allData.Data.Categories.Select(c => c.Id).ToList();

            var excludeIds = new List<int> { allIds.Last() };
            var query = "k";
            var searchResponse = await _client.GetAsync($"api/v1/categories/section/search?query={query}&pageNumber=1&pageSize=10&excludeIds={excludeIds[0]}");

            Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
            var searchData = await searchResponse.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<CategorySummaryResponse>>>();
            Assert.NotNull(searchData);
            Assert.NotNull(searchData.Data);
            Assert.DoesNotContain(searchData.Data.Items, item => excludeIds.Contains(item.Id));
            Assert.All(searchData.Data.Items, item => Assert.Contains(query, item.Name, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<TokenResponse> GetTokenResponseAsync(string email, string password)
        {
            var loginModel = new LoginModel(email, password);
            var response = await _client.PostAsJsonAsync("api/v1/staff/log-in", loginModel);
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();

            if(responseData == null)
                throw new InvalidDataException("Response could not be read");

            return responseData.Data;
        }

        private void SeedData() {
            using(var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();
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
                        IsActive = true,
                        Country = countries[1],
                        StripeCustomerId = "cus_test_2",
                        StripeSubscriptionId = "sub_test_2",
                        SubscriptionPlanId = 1
                    }
                };
                var categories = new List<CategoryEntity>
                {
                    // First category tree      
                    new CategoryEntity
                    {
                        Id = 5,
                        Name = "Mobile",
                        URL = "electronics/mobile",
                        Visibility = true,
                        Level = 2,
                        ParentId = null,
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

                var accounts = new List<StaffEntity>()
            {
                new StaffEntity
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
                new StaffEntity
                {
                    Id = Guid.Parse("5cdb182b-fe81-4c82-bd8e-21362fdd5688"),
                    FirstName = "Stringaa",
                    LastName = "Stringaa",
                    Email = "stringaa@gmail.com",
                    PasswordHash = "C7EA0B35808B733C17F70F1A63BBC7428B62EB71D13608EF2996292A7685A99E-143B98D2518CF95011C47FD9220C51EA",
                    RoleId = 1,
                    TwoFactorEnabled = false,
                    EncryptedTotpSecret = null,
                    ShopId = 1,
                },
                new StaffEntity
                {
                    Id = Guid.Parse("5cdb183b-fe81-4c82-bd8e-21362fdd5688"),
                    FirstName = "Stringb",
                    LastName = "Stringb",
                    Email = "stringb@gmail.com",
                    PasswordHash = "C7EA0B35808B733C17F70F1A63BBC7428B62EB71D13608EF2996292A7685A99E-143B98D2518CF95011C47FD9220C51EA",
                    RoleId = 1,
                    TwoFactorEnabled = false,
                    EncryptedTotpSecret = null,
                    ShopId = 2,
                }
               };

                dbContext.Categories.AddRange(categories);
                dbContext.Staff.AddRange(accounts);
                dbContext.Shops.AddRange(shops);

                dbContext.SaveChanges();

                // Fix PostgreSQL identity sequence after inserting explicit IDs
                dbContext.Database.ExecuteSqlRaw("SELECT setval(pg_get_serial_sequence('\"Categories\"', 'Id'), (SELECT COALESCE(MAX(\"Id\"), 0) FROM \"Categories\"), true);");
            }
        }

        public void Dispose()
        {
            using(var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.Categories.RemoveRange(dbContext.Categories);
                dbContext.Staff.RemoveRange(dbContext.Staff);
                dbContext.Shops.RemoveRange(dbContext.Shops);

                dbContext.SaveChanges();
            }
        }
    }
}
