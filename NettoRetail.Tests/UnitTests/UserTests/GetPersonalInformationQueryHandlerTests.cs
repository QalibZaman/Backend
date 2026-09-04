using MapsterMapper;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.CQRS.Users.Queries.PersonalInformation;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;
using System.Security.Claims;

namespace NettoRetail.Tests.UnitTests.UserTests
{
    public class GetPersonalInformationQueryHandlerTests
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly GetPersonalInformationQueryHandler _handler;

        public GetPersonalInformationQueryHandlerTests()
        {
            _customerRepository = Substitute.For<ICustomerRepository>();
            _staffRepository = Substitute.For<IStaffRepository>();
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _mapper = Substitute.For<IMapper>();

            _handler = new GetPersonalInformationQueryHandler(
                _customerRepository,
                _staffRepository,
                _httpContextAccessor,
                _mapper);
        }

        [Fact]
        public async Task Handle_ShouldReturnCustomerInformation_WhenCustomerExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var customer = CreateCustomer(userId);
            var response = new GetPersonalInformationResponse
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber
            };

            SetUserId(userId);

            _customerRepository
                .GetWithRelatedDataAsync(
                    Arg.Any<GetCustomerByIdSpecification>(),
                    Arg.Any<CancellationToken>())
                .Returns(customer);

            _mapper
                .Map<GetPersonalInformationResponse>(customer)
                .Returns(response);

            // Act
            var result = await _handler.Handle(
                new GetPersonalInformationQuery(),
                CancellationToken.None);

            // Assert
            Assert.Same(response, result);

            await _customerRepository
                .Received(1)
                .GetWithRelatedDataAsync(
                    Arg.Any<GetCustomerByIdSpecification>(),
                    Arg.Any<CancellationToken>());

            await _staffRepository
                .DidNotReceive()
                .GetWithRelatedDataAsync(
                    Arg.Any<GetStaffByIdSpecification>(),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldReturnStaffInformation_WhenCustomerDoesNotExistAndStaffExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var staff = CreateStaff(userId);
            var response = new GetPersonalInformationResponse
            {
                FirstName = staff.FirstName,
                LastName = staff.LastName,
                Email = staff.Email,
                PhoneNumber = staff.PhoneNumber
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

            _mapper
                .Map<GetPersonalInformationResponse>(staff)
                .Returns(response);

            // Act
            var result = await _handler.Handle(
                new GetPersonalInformationQuery(),
                CancellationToken.None);

            // Assert
            Assert.Same(response, result);

            await _customerRepository
                .Received(1)
                .GetWithRelatedDataAsync(
                    Arg.Any<GetCustomerByIdSpecification>(),
                    Arg.Any<CancellationToken>());

            await _staffRepository
                .Received(1)
                .GetWithRelatedDataAsync(
                    Arg.Any<GetStaffByIdSpecification>(),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedException_WhenUserIdClaimIsMissing()
        {
            // Arrange
            SetUserIdClaim(null);

            // Act
            var act = () => _handler.Handle(
                new GetPersonalInformationQuery(),
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

            // Act
            var act = () => _handler.Handle(
                new GetPersonalInformationQuery(),
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

            // Act
            var act = () => _handler.Handle(
                new GetPersonalInformationQuery(),
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

        private static CustomerEntity CreateCustomer(Guid userId)
        {
            return new CustomerEntity
            {
                Id = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+123456789"
            };
        }

        private static StaffEntity CreateStaff(Guid userId)
        {
            return new StaffEntity
            {
                Id = userId,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@example.com",
                PhoneNumber = "+987654321"
            };
        }
    }
}
