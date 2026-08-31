using Microsoft.AspNetCore.Http;
using NettoRetail.Application.CQRS.Users.Commands.PersonalInformation;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces.Services.UserService;
using NettoRetail.Application.Models.Request;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;
using System.Security.Claims;

namespace NettoRetail.Tests.UnitTests.UserTests
{
    public class UpdatePersonalInformationCommandHandlerTests
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly UpdatePersonalInformationCommandHandler _handler;

        public UpdatePersonalInformationCommandHandlerTests()
        {
            _customerRepository = Substitute.For<ICustomerRepository>();
            _staffRepository = Substitute.For<IStaffRepository>();
            _userService = Substitute.For<IUserService>();
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();

            _handler = new UpdatePersonalInformationCommandHandler(
                _customerRepository,
                _staffRepository,
                _userService,
                _httpContextAccessor);
        }

        [Fact]
        public async Task Handle_ShouldUpdateCustomer_WhenCustomerExists()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var customer = CreateCustomer(userId);

            var request = new UpdatePersonalInformationRequest
            {
                FirstName = "Updated",
                LastName = "Customer",
                Email = "updated@example.com",
                PhoneNumber = "123456789",
                CountryId = 1,
                Street = "Main Street",
                HouseNumber = "10",
                Apartment = "5",
                City = "Baku",
                State = "Baku",
                ZipCode = "AZ1000"
            };

            SetUserId(userId);

            _customerRepository
                .GetWithRelatedDataAsync(
                    Arg.Any<GetCustomerByIdSpecification>(),
                    Arg.Any<CancellationToken>())
                .Returns(customer);

            // Act
            await _handler.Handle(
                new UpdatePersonalInformationCommand(request),
                CancellationToken.None);

            // Assert
            await _userService
                .Received(1)
                .UpdatePersonalInformationAsync(
                    customer,
                    request,
                    Arg.Any<CancellationToken>());

            await _customerRepository
                .Received(1)
                .UpdateAsync(
                    customer,
                    Arg.Any<CancellationToken>());

            await _staffRepository
                .DidNotReceive()
                .GetWithRelatedDataAsync(
                    Arg.Any<GetStaffByIdSpecification>(),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldUpdateStaff_WhenCustomerDoesNotExistAndStaffExists()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var staff = CreateStaff(userId);

            var request = new UpdatePersonalInformationRequest
            {
                FirstName = "Updated",
                LastName = "Staff",
                Email = "updated.staff@example.com",
                PhoneNumber = "987654321",
                CountryId = 1,
                Street = "Staff Street",
                HouseNumber = "20",
                Apartment = "2",
                City = "Baku",
                State = "Baku",
                ZipCode = "AZ1001"
            };

            SetUserId(userId);

            _customerRepository
                .GetWithRelatedDataAsync(
                    Arg.Any<GetCustomerByIdSpecification>(),
                    Arg.Any<CancellationToken>())
                .Returns((CustomerEntity?)null);

            _staffRepository
                .GetWithRelatedDataAsync(
                    Arg.Any<GetStaffByIdSpecification>(),
                    Arg.Any<CancellationToken>())
                .Returns(staff);

            // Act
            await _handler.Handle(
                new UpdatePersonalInformationCommand(request),
                CancellationToken.None);

            // Assert
            await _userService
                .Received(1)
                .UpdatePersonalInformationAsync(
                    staff,
                    request,
                    Arg.Any<CancellationToken>());

            await _staffRepository
                .Received(1)
                .UpdateAsync(
                    staff,
                    Arg.Any<CancellationToken>());

            await _customerRepository
                .DidNotReceive()
                .UpdateAsync(
                    Arg.Any<CustomerEntity>(),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedException_WhenUserIdClaimIsMissing()
        {
            // Arrange
            SetUserIdClaim(null);

            var request = CreateRequest();

            // Act
            var act = () => _handler.Handle(
                new UpdatePersonalInformationCommand(request),
                CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(act);

            Assert.Equal(
                "User is not authenticated.",
                exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedException_WhenUserIdClaimIsInvalid()
        {
            // Arrange
            SetUserIdClaim("invalid-guid");

            var request = CreateRequest();

            // Act
            var act = () => _handler.Handle(
                new UpdatePersonalInformationCommand(request),
                CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(act);

            Assert.Equal(
                "User is not authenticated.",
                exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenCustomerAndStaffDoNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            SetUserId(userId);

            _customerRepository
                .GetWithRelatedDataAsync(
                    Arg.Any<GetCustomerByIdSpecification>(),
                    Arg.Any<CancellationToken>())
                .Returns((CustomerEntity?)null);

            _staffRepository
                .GetWithRelatedDataAsync(
                    Arg.Any<GetStaffByIdSpecification>(),
                    Arg.Any<CancellationToken>())
                .Returns((StaffEntity?)null);

            var request = CreateRequest();

            // Act
            var act = () => _handler.Handle(
                new UpdatePersonalInformationCommand(request),
                CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(act);

            Assert.Equal(
                "User not found.",
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

        private static UpdatePersonalInformationRequest CreateRequest()
        {
            return new UpdatePersonalInformationRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "123456789",
                CountryId = 1,
                Street = "Main Street",
                HouseNumber = "10",
                Apartment = "5",
                City = "Baku",
                State = "Baku",
                ZipCode = "AZ1000"
            };
        }

        private static CustomerEntity CreateCustomer(Guid userId)
        {
            return new CustomerEntity
            {
                Id = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };
        }

        private static StaffEntity CreateStaff(Guid userId)
        {
            return new StaffEntity
            {
                Id = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };
        }
    }
}
