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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Tests.IntegrationTests.SuperAdminTests
{
    [Collection("SuperAdminTests")]
    public class EditPlatformAdminTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly HttpClient _Client;
        private readonly IServiceScopeFactory _ScopeFactory;
        private readonly string route = "/api/super-admin/platform-admins";

        private readonly Guid SuperAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private readonly Guid PlatformAdminId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        public EditPlatformAdminTests(CustomWebApplicationFactory factory)
        {
            _Client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            _ScopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

            SeedData();
        }

        [Fact]
        public async Task EditPlatformAdmin_ShouldReturn401Unauthorized()
        { 
            var request = new HttpRequestMessage(HttpMethod.Patch, route);
            var response = await _Client.SendAsync(request);
            request.Content = JsonContent.Create(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task EditPlatformAdmin_ShouldReturn404NotFound()
        {
            var TokenResponse = await GetTokenResponseAsync("/api/auth/login",
                new LoginModel("string@gmail.com", "Stringa1234!"));

            var NonExistingId = Guid.NewGuid();
            var request= new HttpRequestMessage(HttpMethod.Patch, $"{route}/{NonExistingId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TokenResponse.AccessToken);
            request.Content = JsonContent.Create(NonExistingId);

            var response = await _Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);


        }

        [Fact]
        public async Task EditPlatformAdmin_ShouldReturn400BadRequest()
        {
            var TokenResponse = await GetTokenResponseAsync("/api/auth/login",
                new LoginModel("string@gmail.com", "Stringa1234!"));

            var requestModel = new UpdateOrderRequest { OrderId = 999, Status = OrderStatusEnum.Processing };
            var request = new HttpRequestMessage(HttpMethod.Patch, $"{route}/{PlatformAdminId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TokenResponse.AccessToken);
            request.Content = JsonContent.Create(requestModel);

            var response = await _Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task EditPlatformAdmin_ShouldReturn403Forbidden()
        {
           var TokenResponse = await GetTokenResponseAsync("/api/auth/login",
                new LoginModel("platformadmin@example.com", "Stringa1234!"));

            var requestModel = new UpdateOrderRequest { OrderId = 999, Status = OrderStatusEnum.Processing };
            var request = new HttpRequestMessage(HttpMethod.Patch, $"{route}/{PlatformAdminId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", TokenResponse.AccessToken);
            request.Content = JsonContent.Create(requestModel);

            var response = await _Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task EditPlatformAdmin_ShouldReturn200OK()
        {
            var requestModel = new UpdateOrderRequest { OrderId = 999, Status = OrderStatusEnum.Processing };

            var login = new LoginModel("stringb@gmail.com", "Stringa1234!");
            var tokenResponse = await GetTokenResponseAsync("/api/auth/login", login);
            var request = new HttpRequestMessage(HttpMethod.Patch, route);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var response = await _Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task EditPlatformAdmin_ShouldReturnUpdatedPlatformAdmin()
        {
            var requestModel = new UpdateOrderRequest { OrderId = 999, Status = OrderStatusEnum.Processing };
            var login = new LoginModel("stringb@gmail.com", "Stringa1234!");
            var tokenResponse = await GetTokenResponseAsync("/api/auth/login", login);
            var request = new HttpRequestMessage(HttpMethod.Patch, route);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
            request.Content = JsonContent.Create(requestModel);

            var response = await _Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var UpdatedPermission = await GetAdminPermissionsAsync(PlatformAdminId);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task<List<int>> GetAdminPermissionsAsync(Guid platformAdminId)
        {
            using var scope = _ScopeFactory.CreateScope();
            var dbcontext = scope.ServiceProvider.GetRequiredService<DbContext>();

            var account = await dbcontext.Set<StaffEntity>()
                .Include(s => s.Role)
                .ThenInclude(r => r.PermissionRoles)
                .FirstOrDefaultAsync(s => s.Id == platformAdminId);

            return account?.Role.PermissionRoles.Select(pr => pr.PermissionId).ToList() ?? new List<int>();
        }

        private async Task<TokenResponse> GetTokenResponseAsync(string loginPath, LoginModel login)
        {
            var response = await _Client.PostAsJsonAsync(loginPath, login);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>();
            return responseData?.Data ?? throw new InvalidDataException("Token data was null");
        }

        private void SeedData()
        {
            using var scope = _ScopeFactory.CreateScope();
            var dbcontext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var admins = dbcontext.Staff.Where(s => s.Role.Name == "PlatformAdmin").ToList();
            var superAdmin = dbcontext.Staff.FirstOrDefault(s => s.Role.Name == "SuperAdmin");
            var roles = new List<RoleEntity>
            {
                new RoleEntity
                {
                    Id = 1,
                    Name = "PlatformAdmin",
                    Description = "Platform Admin role with permissions to manage the platform",
                    ShopId = 1,
                    Shop = dbcontext.Shops.FirstOrDefault(s => s.Id == 1),
                    PermissionRoles = new List<PermissionRoleEntity>
                    {
                        new PermissionRoleEntity
                        {
                            PermissionId = 1,
                            RoleId = 1,
                        },
                        new PermissionRoleEntity
                        {
                            PermissionId = 2,
                            RoleId = 1
                        },
                        new PermissionRoleEntity
                        {
                            PermissionId = 3,
                            RoleId = 1
                        }

                    }
                }
            };

            var accounts = new List<StaffEntity>
            {
                new StaffEntity
                {
                    Id = PlatformAdminId,
                    Email = "platformadmin@example.com",
                    FirstName = "Platform",
                    LastName = "Admin",
                    PasswordHash = "hashedpassword",
                    RoleId = 2,
                    Role = roles[1],
                    TwoFactorEnabled = false,
                },

                new StaffEntity
                {
                    Id = SuperAdminId,
                    Email = "superadmin@example.com",
                    FirstName = "Super",
                    LastName = "Admin",
                    PasswordHash = "hashedpassword213",
                    RoleId = 1,
                    Role = roles[0],
                    TwoFactorEnabled = false,
                }
            };

            dbcontext.Roles.AddRange(roles);
            dbcontext.Staff.AddRange(accounts);
            dbcontext.SaveChanges();
        }
        
        public void Dispose()
        {
            using var scope = _ScopeFactory.CreateScope();
            var dbcontext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            dbcontext.Roles.RemoveRange(dbcontext.Roles);
            dbcontext.Staff.RemoveRange(dbcontext.Staff);
            
            dbcontext.SaveChanges();
        }
    }
}




