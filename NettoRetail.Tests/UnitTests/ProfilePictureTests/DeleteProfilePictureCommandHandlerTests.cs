using Microsoft.AspNetCore.Http;
using NettoRetail.Application.CQRS.Users.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Tests.UnitTests.ProfilePictureTests
{
    public class DeleteProfilePictureCommandHandlerTests
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAzureBlobService _azureBlobService;

        private readonly DeleteProfilePictureCommandHandler _handler;

        public DeleteProfilePictureCommandHandlerTests()
        {
            _customerRepository = Substitute.For<ICustomerRepository>();
            _staffRepository = Substitute.For<IStaffRepository>();
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _azureBlobService = Substitute.For<IAzureBlobService>();

            _handler = new DeleteProfilePictureCommandHandler(
                _customerRepository,
                _staffRepository,
                _httpContextAccessor,
                _azureBlobService);
        }

        [Fact]
        public async Task Handle_ShouldDeleteProfilePicture_WhenUserIsCustomer()
        {
            var userId = Guid.NewGuid();
            var imageUrl = "https://storage/profile-image.jpg";

            var customer = CreateCustomer(
                userId,
                imageUrl);

            SetupAuthenticatedUser(userId);

            _customerRepository
                .GetWithRelatedDataAsync(
                    Arg.Any<GetCustomerByIdSpecification>(),
                    Arg.Any<CancellationToken>())
                .Returns(customer);

            await _handler.Handle(
                new DeleteProfilePictureCommand(),
                CancellationToken.None);

            await _azureBlobService
                .Received(1)
                .DeleteByUrlAsync(
                    imageUrl,
                    Arg.Any<CancellationToken>());

            Assert.Null(customer.ProfileImageUrl);

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
        public async Task Handle_ShouldDeleteProfilePicture_WhenUserIsStaff()
        {
            var userId = Guid.NewGuid();
            var imageUrl = "https://storage/profile-image.jpg";

            var staff = CreateStaff(
                userId,
                imageUrl);

            SetupAuthenticatedUser(userId);

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

            await _handler.Handle(
                new DeleteProfilePictureCommand(),
                CancellationToken.None);

            await _azureBlobService
                .Received(1)
                .DeleteByUrlAsync(
                    imageUrl,
                    Arg.Any<CancellationToken>());

            Assert.Null(staff.ProfileImageUrl);

            await _staffRepository
                .Received(1)
                .UpdateAsync(
                    staff,
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorized_WhenUserIdIsMissing()
        {
            SetupAuthenticatedUser(null);

            var act = () => _handler.Handle(
                new DeleteProfilePictureCommand(),
                CancellationToken.None);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(act);

            Assert.Equal(
                "User is not authenticated.",
                exception.Message);

            await _azureBlobService
                .DidNotReceive()
                .DeleteByUrlAsync(
                    Arg.Any<string>(),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorized_WhenUserIdIsInvalid()
        {
            SetupAuthenticatedUser("not-a-guid");

            var act = () => _handler.Handle(
                new DeleteProfilePictureCommand(),
                CancellationToken.None);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(act);

            Assert.Equal(
                "User is not authenticated.",
                exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            var userId = Guid.NewGuid();

            SetupAuthenticatedUser(userId);

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

            var act = () => _handler.Handle(
                new DeleteProfilePictureCommand(),
                CancellationToken.None);

            var exception = await Assert.ThrowsAsync<NotFoundException>(act);

            Assert.Equal(
                "User not found.",
                exception.Message);

            await _azureBlobService
                .DidNotReceive()
                .DeleteByUrlAsync(
                    Arg.Any<string>(),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequest_WhenCustomerHasNoProfilePicture()
        {
            var userId = Guid.NewGuid();

            var customer = CreateCustomer(
                userId,
                profileImageUrl: null);

            SetupAuthenticatedUser(userId);

            _customerRepository
                .GetWithRelatedDataAsync(
                    Arg.Any<GetCustomerByIdSpecification>(),
                    Arg.Any<CancellationToken>())
                .Returns(customer);

            var act = () => _handler.Handle(
                new DeleteProfilePictureCommand(),
                CancellationToken.None);

            var exception = await Assert.ThrowsAsync<BadRequestException>(act);

            Assert.Equal(
                "Profile image does not exist.",
                exception.Message);

            await _azureBlobService
                .DidNotReceive()
                .DeleteByUrlAsync(
                    Arg.Any<string>(),
                    Arg.Any<CancellationToken>());

            await _customerRepository
                .DidNotReceive()
                .UpdateAsync(
                    Arg.Any<CustomerEntity>(),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequest_WhenStaffHasNoProfilePicture()
        {
            var userId = Guid.NewGuid();

            var staff = CreateStaff(
                userId,
                profileImageUrl: null);

            SetupAuthenticatedUser(userId);

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

            var act = () => _handler.Handle(
                new DeleteProfilePictureCommand(),
                CancellationToken.None);

            var exception = await Assert.ThrowsAsync<BadRequestException>(act);

            Assert.Equal(
                "Profile image does not exist.",
                exception.Message);

            await _azureBlobService
                .DidNotReceive()
                .DeleteByUrlAsync(
                    Arg.Any<string>(),
                    Arg.Any<CancellationToken>());

            await _staffRepository
                .DidNotReceive()
                .UpdateAsync(
                    Arg.Any<StaffEntity>(),
                    Arg.Any<CancellationToken>());
        }

        private void SetupAuthenticatedUser(Guid userId)
        {
            SetupAuthenticatedUser(userId.ToString());
        }

        private void SetupAuthenticatedUser(string? userId)
        {
            var context = new DefaultHttpContext();

            var claims = userId is null
                ? []
                : new[]
                {
                new Claim(ClaimTypes.NameIdentifier, userId)
                };

            context.User = new ClaimsPrincipal(
                new ClaimsIdentity(claims, "TestAuthentication"));

            _httpContextAccessor.HttpContext.Returns(context);
        }

        private static CustomerEntity CreateCustomer(
            Guid id,string? profileImageUrl = null)
        {
            return new CustomerEntity
            {
                Id = id,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PasswordHash = "test-password-hash",
                ProfileImageUrl = profileImageUrl
            };
        }

        private static StaffEntity CreateStaff(
            Guid id,string? profileImageUrl = null)
        {
            return new StaffEntity
            {
                Id = id,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@example.com",
                PasswordHash = "test-password-hash",
                ProfileImageUrl = profileImageUrl
            };
        }
    }
}
