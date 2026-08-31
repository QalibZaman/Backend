using Microsoft.AspNetCore.Http;
using NettoRetail.Application.CQRS.Users.Commands.Onboarding;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;
using System.Security.Claims;

namespace NettoRetail.Tests.UnitTests.OnboardingTests
{
    public class CompleteOnboardingCommandHandlerTests
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CompleteOnboardingCommandHandler _handler;

        public CompleteOnboardingCommandHandlerTests()
        {
            _staffRepository = Substitute.For<IStaffRepository>();
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();

            _handler = new CompleteOnboardingCommandHandler(
                _staffRepository,
                _httpContextAccessor);
        }

        [Fact]
        public async Task Handle_ShouldCompleteOnboarding_WhenStaffExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var staff = CreateStaff(userId);

            SetUserId(userId);

            _staffRepository
                .GetAsync(
                    Arg.Any<GetStaffByIdSpecification>(),
                    Arg.Any<CancellationToken>())
                .Returns(staff);

            // Act
            await _handler.Handle(
                new CompleteOnboardingCommand(),
                CancellationToken.None);

            // Assert
            Assert.True(staff.IsOnboardingCompleted);

            await _staffRepository
                .Received(1)
                .UpdateAsync(
                    staff,
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedException_WhenUserIdClaimIsMissing()
        {
            // Arrange
            SetUserIdClaim(null);

            // Act
            var act = () => _handler.Handle(
                new CompleteOnboardingCommand(),
                CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<MyUnauthorizedException>(act);

            Assert.Equal("User is not authorized.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedException_WhenUserIdClaimIsInvalid()
        {
            // Arrange
            SetUserIdClaim("invalid-guid");

            // Act
            var act = () => _handler.Handle(
                new CompleteOnboardingCommand(),
                CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<MyUnauthorizedException>(act);

            Assert.Equal("User is not authorized.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenStaffDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            SetUserId(userId);

            _staffRepository
                .GetAsync(
                    Arg.Any<GetStaffByIdSpecification>(),
                    Arg.Any<CancellationToken>())
                .Returns((StaffEntity?)null);

            // Act
            var act = () => _handler.Handle(
                new CompleteOnboardingCommand(),
                CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(act);

            Assert.Equal(
                "Staff account was not found.",
                exception.Message);
        }

        private void SetUserId(Guid userId)
        {
            SetUserIdClaim(userId.ToString());
        }

        private void SetUserIdClaim(string? userId)
        {
            var claims = string.IsNullOrWhiteSpace(userId)
                ? []
                : new[]
                {
            new Claim(ClaimTypes.NameIdentifier, userId)
                };

            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = principal
            };

            _httpContextAccessor.HttpContext.Returns(httpContext);
        }

        private static StaffEntity CreateStaff(Guid userId)
        {
            return new StaffEntity
            {
                Id = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
            };
        }
    }
}
