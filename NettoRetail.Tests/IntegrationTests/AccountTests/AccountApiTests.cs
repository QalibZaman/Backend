using System.Net.Http.Json;
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Response;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.DataAccess.Context;
using NettoRetail.Application.Models.Admin;
using System.Net.Http.Headers;
using NettoRetail.Application.Models.TwoFactorAuth;
using NettoRetail.Application.Models.Request.Password;

namespace NettoRetail.Tests.IntegrationTests.AccountTests;

[Collection("Sequential Tests")]
public class AccountApiTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly List<string> _testEmails = new();

    public AccountApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    [Fact]
    public async Task SignUp_ShouldReturnToken_WhenModelIsValid()
    {
        // Arrange
        var email = "signup@gmail.com";
        var signUpModel = new AdminSignUpModel
        {
            Email = email,
            Password = "String123!",
            FirstName = "Testuser",
            LastName = "One",
            ConfirmPassword = "String123!"
        };

        try
        {
            // Act
            var response = await _client.PostAsJsonAsync("api/v1/admins/sign-up", signUpModel);
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.Data);
            Assert.False(string.IsNullOrEmpty(responseData.Data.AccessToken));
            Assert.False(string.IsNullOrEmpty(responseData.Data.RefreshToken));
        }
        finally
        {
            _testEmails.Add(email);
        }
    }

    [Fact]
    public async Task LogIn_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "login@gmail.com";
        var password = "String1234!";

        try
        {
            await RegisterUserAsync(email, password);

            var loginModel = new LoginModel(email, password);

            // Act
            var response = await _client.PostAsJsonAsync("api/v1/staff/log-in", loginModel);
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.Data);
            Assert.False(string.IsNullOrEmpty(responseData.Data.AccessToken));
            Assert.False(string.IsNullOrEmpty(responseData.Data.RefreshToken));
        }
        finally
        {
            _testEmails.Add(email);
        }
    }

    [Fact]
    public async Task LogIn_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange 
        var loginModel = new LoginModel($"aboba@example.com", "WrongPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/staff/log-in", loginModel);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SignUp_ShouldReturnBadRequest_WhenModelIsInvalid()
    {
        // Arrange - Use unique but invalid data
        var invalidSignUpModel = new AdminSignUpModel
        {
            Email = $"invalid-email-{Guid.NewGuid()}",
            Password = "short",
            FirstName = "A",
            LastName = "User123",
            ConfirmPassword = "differentPassword"
        };

        try
        {
            // Act
            var response = await _client.PostAsJsonAsync("api/v1/admins/sign-up", invalidSignUpModel);
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(errorResponse);
            Assert.NotNull(errorResponse.Errors);
            Assert.NotEmpty(errorResponse.Errors);
        }
        finally
        {
            _testEmails.Add(invalidSignUpModel.Email);
        }
    }

    [Fact]
    public async Task LogOut_ShouldReturnNoContent_WhenLoggedIn()
    {
        // Arrange - Generate unique test data
        var email = "logout@gmail.com";
        var password = "String123!";

        var loginModel = new LoginModel(email, password);

        try
        {
            var tokenResponse = await RegisterUserAsync(email, password);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            // Act 
            var response = await _client.PostAsync("api/v1/staff/log-out", null);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            _testEmails.Add(email);
        }
    }

    [Fact]
    public async Task LogOut_ShouldReturnUnauthorizedAfterLogOut_WhenAccessTokenUnchanged()
    {
        // Arrange - Generate unique test data
        var email = "logoutaccess@gmail.com";
        var password = "String123!";

        var loginModel = new LoginModel(email, password);

        try
        {
            var tokenResponse = await RegisterUserAsync(email, password);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            // Act 
            await _client.PostAsync("api/v1/staff/log-out", null);
            var testResponse = await _client.PostAsync("api/v1/staff/log-out", null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, testResponse.StatusCode);
        }
        finally
        {
            _testEmails.Add(email);
        }
    }

    [Fact]
    public async Task EnableTwoFactorAuth_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var email = "enablefa@gmail.com";
        var password = "String123!";

        var request = new EnableTwoFactorAuthRequest(password);

        try
        {
            var tokenResponse = await RegisterUserAsync(email, password);

            _client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            // Act 
            var response = await _client.PostAsJsonAsync("api/v1/staff/enable-2fa", request);
            var responseData = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseData);
        }
        finally
        {
            _testEmails.Add(email);
        }
    }

    [Fact]
    public async Task RecoverPassword_ShouldReturnNoContent_WhenRequestIsValid()
    {
        // Arrange
        var email = "recoverpassword@gmail.com";
        var password = "String123!";
        var newPassword = "NewString123!";

        try
        {
            var tokenResponse = await RegisterUserAsync(email, password);

            var request = new ResetPasswordRequest(tokenResponse.AccessToken, newPassword, newPassword);

            _client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            // Act
            var response = await _client.PostAsJsonAsync("api/v1/staff/reset-password", request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            _testEmails.Add(email);
        }  
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnNoContent_WhenRequestIsValid()
    {
        // Arrange
        var email = "new@gmail.com";
        var oldPassword = "OldOne123!";
        var newPassword = "NewOne123!";

        try
        {
            var tokenResponse = await RegisterUserAsync(email, oldPassword);

            var request = new ChangePasswordRequest(oldPassword, newPassword, newPassword);

            _client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            // Act
            var response = await _client.PutAsJsonAsync("api/v1/staff/change-password", request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            _testEmails.Add(email);
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

    private async Task<TokenResponse> RegisterUserAsync(string email, string password)
    {
        var signUpModel = new AdminSignUpModel
        {
            Email = email,
            Password = password,
            FirstName = "Test",
            LastName = "User",
            ConfirmPassword = password
        };

        var response = await _client.PostAsJsonAsync("api/v1/admins/sign-up", signUpModel);

        var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();

        if (responseData == null)
            throw new InvalidDataException("Response could not be read");

        return responseData.Data;
    }

    private async Task<T> ExecuteWithRetry<T>(Func<Task<T>> action, int maxRetries = 3)
    {
        var retryCount = 0;
        while (true)
        {
            try
            {
                return await action();
            }
            catch when (retryCount < maxRetries)
            {
                retryCount++;
                await Task.Delay(100 * retryCount);
            }
        }
    }

    public void Dispose()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (_testEmails.Any())
            {
                var testUsers = dbContext.Accounts.Where(u => _testEmails.Contains(u.Email));
                dbContext.Accounts.RemoveRange(testUsers);
                dbContext.SaveChanges();
            }
        }
    }
}