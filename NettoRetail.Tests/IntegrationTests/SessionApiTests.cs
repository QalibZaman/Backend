using System.Net.Http.Json;
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Context;
using NettoRetail.Application.Models.Entities;
using System.Net.Http.Headers;

namespace NettoRetail.Tests.IntegrationTests;

public class SessionApiTests : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly List<string> _testEmails = new();

    public SessionApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    [Fact]
    public async Task GetSessions_ShouldHaveCountOne_WhenRegistered()
    {
        // Arrange
        var email = "signups@gmail.com";
        var password = "String123!";

        try
        {
            // Act
            var tokenResponse = await RegisterUserAsync(email, password);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var sessionResponse = await _client.GetAsync("api/v1/sessions");
            var sessionData = await sessionResponse.Content
                .ReadFromJsonAsync<ApiResponse<ICollection<SessionModel>>>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);
            Assert.NotNull(tokenResponse);
            Assert.False(string.IsNullOrEmpty(tokenResponse.AccessToken));
            Assert.False(string.IsNullOrEmpty(tokenResponse.RefreshToken));
            Assert.True(sessionData?.Data.Count == 1);
        }
        finally
        {
            _testEmails.Add(email);
        }
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
