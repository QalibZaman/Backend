using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Payments;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Context;
using NettoRetail.Domain.Entities;
using Stripe;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NettoRetail.Tests.IntegrationTests.PaymentTests
{
    public class PaymentApiTest : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceScopeFactory _factory;

        public PaymentApiTest(CustomWebApplicationFactory factory)
        {

            _httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true
            });
            _factory = factory.Services.GetRequiredService<IServiceScopeFactory>();
            SeedData();
        }
        [Fact]
        public async Task AddPayment_ShouldReturnNoContent_IfInputsAreValid()
        {
            // Arrange
            var email = "stringaa@gmail.com";
            var password = "Stringa1234!";
            // Act
            var tokenResponse = await GetTokenResponseAsync(email, password);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
            var methodId = await CreateStripePaymentMethodAsync("tok_visa_debit");
            var request = new AddPaymentRequest
            {
                PaymentMethodId = methodId,
            };
            var response = await _httpClient.PostAsJsonAsync("api/v1/payments", request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        }
        [Fact]
        public async Task AddPayment_ShouldReturnBadRequest_IfInputsAreInvalid()
        {
            // Arrange
            var email = "stringaa@gmail.com";
            var password = "Stringa1234!";
            // Act
            var tokenResponse = await GetTokenResponseAsync(email, password);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
            var request = new AddPaymentRequest
            {
                PaymentMethodId = "1",
            };
            var response = await _httpClient.PostAsJsonAsync("api/v1/payments", request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        }
        [Fact]
        public async Task EditPayment_ShouldReturnBadRequest_IfStripePaymentMethodIdIsEmpty()
        {
            // Arrange
            var email = "stringaa@gmail.com";
            var password = "Stringa1234!";

            var tokenResponse = await GetTokenResponseAsync(email, password);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var request = new EditPaymentRequest
            {
                StripePaymentMethodId = "",
                CardHolderName = "Test User"
            };

            // Act
            var response = await _httpClient.PutAsJsonAsync("api/v1/payments", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        [Fact]
        public async Task EditPayment_ShouldReturnUnauthorized_IfTokenIsMissing()
        {
            // Arrange
            var request = new EditPaymentRequest
            {
                StripePaymentMethodId = "pm_test_123",
                CardHolderName = "Test User"
            };

            // Act
            var response = await _httpClient.PutAsJsonAsync("api/v1/payments", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        [Fact]
        public async Task EditPayment_ShouldReturnOk_IfInputsAreValid()
        {
            // Arrange
            var email = "stringaa@gmail.com";
            var password = "Stringa1234!";

            var tokenResponse = await GetTokenResponseAsync(email, password);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var methodId = await CreateStripePaymentMethodAsync("tok_visa_debit");
            var addRequest = new AddPaymentRequest
            {
                PaymentMethodId = methodId
            };

            var addResponse = await _httpClient.PostAsJsonAsync("api/v1/payments", addRequest);
            Assert.Equal(HttpStatusCode.NoContent, addResponse.StatusCode);

            // edit payment
            var editRequest = new EditPaymentRequest
            {
                StripePaymentMethodId = methodId,
                CardHolderName = "Shahin Ismayilov"
            };

            // Act
            var response = await _httpClient.PutAsJsonAsync("api/v1/payments", editRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task DeletePayment_ShouldReturnOk_IfInputsAreValid()
        {
            // Arrange
            var email = "stringaa@gmail.com";
            var password = "Stringa1234!";

            var tokenResponse = await GetTokenResponseAsync(email, password);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
            var methodId = await CreateStripePaymentMethodAsync("tok_visa_debit");
            var request = new AddPaymentRequest
            {
                PaymentMethodId = methodId,
            };

            var addResponse = await _httpClient.PostAsJsonAsync("api/v1/payments", request);
            Assert.Equal(HttpStatusCode.NoContent, addResponse.StatusCode);

            // Act
            var response = await _httpClient.DeleteAsync(
                $"api/v1/payments/{methodId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeletePayment_ShouldReturnUnauthorized_IfTokenMissing()
        {
            // Arrange
            var paymentMethodId = "pm_card_visa";

            // Act
            var response = await _httpClient.DeleteAsync(
                $"api/v1/payments/{paymentMethodId}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeletePayment_ShouldReturnNotFound_IfPaymentNotExists()
        {
            // Arrange
            var email = "stringaa@gmail.com";
            var password = "Stringa1234!";

            var tokenResponse = await GetTokenResponseAsync(email, password);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var paymentMethodId = "pm_not_existing";

            // Act
            var response = await _httpClient.DeleteAsync(
                $"api/v1/payments/{paymentMethodId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        [Fact]
        public async Task ChangeDefaultPayment_ShouldReturnNoContent_IfInputsAreValid()
        {
            var email = "stringaa@gmail.com";
            var password = "Stringa1234!";

            var token = await GetTokenResponseAsync(email, password);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var methodId = await CreateStripePaymentMethodAsync("tok_visa_debit");
            var secMethodId = await CreateStripePaymentMethodAsync("tok_mastercard_debit");


            var requestVisa = new AddPaymentRequest
            {
                PaymentMethodId = methodId
            };
            var requestMaster = new AddPaymentRequest
            {
                PaymentMethodId = secMethodId,
            };

            await _httpClient.PostAsJsonAsync("api/v1/payments", requestVisa);
            await _httpClient.PostAsJsonAsync("api/v1/payments", requestMaster);


            var response = await _httpClient.PutAsync(
                $"api/v1/payments/{requestMaster.PaymentMethodId}/set-default",
                null);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        [Fact]
        public async Task ChangeDefaultPayment_ShouldReturNotFound_IfOnlyOnePaymentMethod()
        {
            var email = "stringaa@gmail.com";
            var password = "Stringa1234!";

            var token = await GetTokenResponseAsync(email, password);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.AccessToken);


            var pm = "pm_card_visa";

            await _httpClient.PostAsJsonAsync(
                "api/v1/payments",
                pm);


            var response = await _httpClient.PutAsync(
                $"api/v1/payments/{pm}/set-default",
                null);


            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        [Fact]
        public async Task ChangeDefaultPayment_ShouldReturnNotFound_IfPaymentBelongsToAnotherUser()
        {
            var firstToken = await GetTokenResponseAsync(
                "stringaa@gmail.com",
                "Stringa1234!");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", firstToken.AccessToken);

            var pmVisa = "pm_card_visa";
            var pmMaster = "pm_card_master";

            await _httpClient.PostAsJsonAsync("api/v1/payments", pmVisa);
            await _httpClient.PostAsJsonAsync("api/v1/payments", pmMaster);


            // user2
            var secondToken = await GetTokenResponseAsync(
                "stringa@gmail.com",
                "Stringa1234!");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", secondToken.AccessToken);


            var response = await _httpClient.PutAsync(
                $"api/v1/payments/{pmVisa}/set-default",
                null);


            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        private async Task<TokenResponse> GetTokenResponseAsync(string email, string password)
        {
            var loginModel = new LoginModel(email, password);
            var response = await _httpClient.PostAsJsonAsync("api/v1/staff/log-in", loginModel);
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();

            if (responseData == null)
                throw new InvalidDataException("Response could not be read");

            return responseData.Data;
        }
        private async Task<string> CreateStripePaymentMethodAsync(string token)
        {
            var service = new PaymentMethodService();

            var pm = await service.CreateAsync(
                new PaymentMethodCreateOptions
                {
                    Type = "card",
                    Card = new PaymentMethodCardOptions
                    {
                        Token = token
                    }
                });

            return pm.Id;
        }
        private void SeedData()
        {
            using (var scope = _factory.CreateScope())
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
                }
               };

                dbContext.Categories.AddRange(categories);
                dbContext.Staff.AddRange(accounts);
                dbContext.Shops.AddRange(shops);

                dbContext.SaveChanges();
            }

        }
        public void Dispose()
        {
            using (var scope = _factory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.PaymentMethods.RemoveRange(dbContext.PaymentMethods);
                dbContext.Categories.RemoveRange(dbContext.Categories);
                dbContext.Staff.RemoveRange(dbContext.Staff);
                dbContext.Shops.RemoveRange(dbContext.Shops);

                dbContext.SaveChanges();
            }
        }
    }
}
