using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Services.ISessionModelFactory;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.SessionTests
{
    public class SessionModelFactoryTests
    {
        private readonly IStaffRepository _staffRepository = Substitute.For<IStaffRepository>();
        private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
        private readonly SessionModelFactory _factory;

        public SessionModelFactoryTests()
        {
            _staffRepository
                .GetAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
                .Returns(new StaffEntity
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Test",
                    LastName = "Testtest",
                    Email = "test@gmail.com"
                });

            _factory = new SessionModelFactory(_staffRepository, _customerRepository);
        }

        [Theory]
        [InlineData("Mountain View", "US", "Mountain View, United States")]
        [InlineData("Warsaw", "PL", "Warsaw, Poland")]
        [InlineData(null, "US", "Unknown, United States")]
        [InlineData("Mountain View", null, "Mountain View, Unknown")]
        [InlineData(null, null, "Unknown, Unknown")]
        [InlineData("  Warsaw  ", "PL", "Warsaw, Poland")]
        public async Task CreateSessionModel_ShouldFormatLocationAsCityCountry(
            string? city, string? country, string expected)
        {
            var result = await CreateModel(new Location(city, country));

            Assert.Equal(expected, result.Location);
        }

        [Fact]
        public async Task CreateSessionModel_ShouldReturnUnknown_WhenLocationIsNull()
        {
            var result = await CreateModel(null);

            Assert.Equal("Unknown, Unknown", result.Location);
        }

        [Fact]
        public async Task CreateSessionModel_ShouldKeepCountryCode_WhenCodeIsNotValidRegion()
        {
            var result = await CreateModel(new Location("Somewhere", "ZZZZ"));

            Assert.Equal("Somewhere, ZZZZ", result.Location);
        }

        [Fact]
        public async Task CreateSessionModel_ShouldLimitLocationTo100CharactersWithoutTrailingSpace()
        {
            var longCity = new string('a', 98) + " b";

            var result = await CreateModel(new Location(longCity, "US"));

            Assert.True(result.Location.Length <= 100);
            Assert.Equal(result.Location.TrimEnd(), result.Location);
        }

        private Task<NettoRetail.Application.Models.Entities.SessionModel> CreateModel(Location? location) =>
            _factory.CreateSessionModel(
                "Mozilla/5.0 (Windows NT 10.0) Chrome/120.0",
                "8.8.8.8",
                CreateJwt(),
                "refresh-token",
                location,
                DateTime.UtcNow.AddDays(1),
                UserTypeEnum.Admin,
                CancellationToken.None);

        private static string CreateJwt()
        {
            var key = new SymmetricSecurityKey(new byte[32]);
            var token = new JwtSecurityToken(
                claims: [new Claim("email", "test@gmail.com")],
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}