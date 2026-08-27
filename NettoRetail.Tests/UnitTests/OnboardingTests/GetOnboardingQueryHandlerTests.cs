using Microsoft.AspNetCore.Http;
using NettoRetail.Application.CQRS.Users.Queries.Onboarding;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;
using System.Security.Claims;

namespace NettoRetail.Tests.UnitTests.OnboardingTests
{
    public class GetOnboardingQueryHandlerTests
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetOnboardingQueryHandler _handler;

        public GetOnboardingQueryHandlerTests()
        {
            _staffRepository = Substitute.For<IStaffRepository>();
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();

            _handler = new GetOnboardingQueryHandler(
                _staffRepository,
                _httpContextAccessor);
        }

        [Fact]
        public async Task Handle_ShouldReturnOnboarding_WhenOnboardingIsNotCompleted()
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
            var result = await _handler.Handle(new GetOnboardingQuery(),CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result!.OnboardingSeen);
            Assert.Equal(2, result.Slides.Count);

            Assert.Equal(1, result.Slides[0].SlideId);
            Assert.Equal(
                OnboardingConstants.Welcome.Title,
                result.Slides[0].Title);
            Assert.Equal(
                OnboardingConstants.Welcome.ImageUrl,
                result.Slides[0].ImageUrl);
            Assert.Equal(
                OnboardingConstants.Welcome.Headline,
                result.Slides[0].Headline);
            Assert.Equal(
                OnboardingConstants.Welcome.DetailedText,
                result.Slides[0].DetailedText);

            Assert.Equal(2, result.Slides[1].SlideId);
            Assert.Equal(
                OnboardingConstants.Navigation.Title,
                result.Slides[1].Title);
            Assert.Equal(
                OnboardingConstants.Navigation.ImageUrl,
                result.Slides[1].ImageUrl);
            Assert.Equal(
                OnboardingConstants.Navigation.Headline,
                result.Slides[1].Headline);
            Assert.Equal(
                OnboardingConstants.Navigation.DetailedText,
                result.Slides[1].DetailedText);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenOnboardingIsCompleted()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var staff = CreateStaff(userId);
            staff.IsOnboardingCompleted = true;

            SetUserId(userId);

            _staffRepository
                .GetAsync(
                    Arg.Any<GetStaffByIdSpecification>(),
                    Arg.Any<CancellationToken>())
                .Returns(staff);

            // Act
            var result = await _handler.Handle(new GetOnboardingQuery(),CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedException_WhenUserIdClaimIsMissing()
        {
            // Arrange
            SetUserIdClaim(null);

            // Act
            var act = () => _handler.Handle(new GetOnboardingQuery(),CancellationToken.None);

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
            var act = () => _handler.Handle(new GetOnboardingQuery(),CancellationToken.None);

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
            var act = () => _handler.Handle(new GetOnboardingQuery(),CancellationToken.None);

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
                PasswordHash = "test_hash"
            };
        }
    }
}
