using Microsoft.AspNetCore.Http;
using NettoRetail.Application.CQRS.Users.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services.IImageValidatorService;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;
using System.Security.Claims;

namespace NettoRetail.Tests.UnitTests.ProfilePictureTests
{
    public class UploadProfilePictureCommandHandlerTests
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAzureBlobService _azureBlobService;
        private readonly IImageValidatorService _imageValidatorService;

        private readonly UploadProfilePictureCommandHandler _handler;

        public UploadProfilePictureCommandHandlerTests()
        {
            _customerRepository = Substitute.For<ICustomerRepository>();
            _staffRepository = Substitute.For<IStaffRepository>();
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _azureBlobService = Substitute.For<IAzureBlobService>();
            _imageValidatorService = Substitute.For<IImageValidatorService>();

            _handler = new UploadProfilePictureCommandHandler(
                _customerRepository,
                _staffRepository,
                _httpContextAccessor,
                _azureBlobService,
                _imageValidatorService);
        }

        [Fact]
        public async Task Handle_ShouldUploadAndSetProfilePicture_WhenUserIsCustomer()
        {
            var userId = Guid.NewGuid();
            var customer = CreateCustomer(userId);
            var imageUrl = "https://storage/profile-image.jpg";

            SetupAuthenticatedUser(userId);

            _customerRepository
                .GetWithRelatedDataAsync(
                    Arg.Any<GetCustomerByIdSpecification>(),
                    Arg.Any<CancellationToken>())
                .Returns(customer);

            _azureBlobService
                .UploadAsync(
                    Arg.Any<string>(),
                    Arg.Any<Stream>(),
                    Arg.Any<string>(),
                    Arg.Any<BlobContentCategoryEnum>(),
                    Arg.Any<bool>(),
                    Arg.Any<string?>())
                .Returns(imageUrl);

            var command = CreateCommand();

            await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(imageUrl, customer.ProfileImageUrl);

            await _imageValidatorService
                .Received(1)
                .ValidateProfileImageAsync(
                    command.Model.File,
                    Arg.Any<CancellationToken>());

            await _azureBlobService
                .Received(1)
                .UploadAsync(
                    Arg.Any<string>(),
                    Arg.Any<Stream>(),
                    "dot-net",
                    BlobContentCategoryEnum.Image,
                    true,
                    command.Model.File.ContentType);

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
        public async Task Handle_ShouldUploadAndSetProfilePicture_WhenUserIsStaff()
        {
            var userId = Guid.NewGuid();
            var staff = CreateStaff(userId);
            var imageUrl = "https://storage/profile-image.png";

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

            _azureBlobService
                .UploadAsync(
                    Arg.Any<string>(),
                    Arg.Any<Stream>(),
                    Arg.Any<string>(),
                    Arg.Any<BlobContentCategoryEnum>(),
                    Arg.Any<bool>(),
                    Arg.Any<string?>())
                .Returns(imageUrl);

            var command = CreateCommand("profile.png");

            await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(imageUrl, staff.ProfileImageUrl);

            await _imageValidatorService
                .Received(1)
                .ValidateProfileImageAsync(
                    command.Model.File,
                    Arg.Any<CancellationToken>());

            await _azureBlobService
                .Received(1)
                .UploadAsync(
                    Arg.Any<string>(),
                    Arg.Any<Stream>(),
                    "dot-net",
                    BlobContentCategoryEnum.Image,
                    true,
                    command.Model.File.ContentType);

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

            var command = CreateCommand();

            var act = () => _handler.Handle(
                command,
                CancellationToken.None);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(act);

            Assert.Equal(
                "User is not authenticated.",
                exception.Message);

            await _imageValidatorService
                .DidNotReceive()
                .ValidateProfileImageAsync(
                    Arg.Any<IFormFile>(),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorized_WhenUserIdIsInvalid()
        {
            SetupAuthenticatedUser("not-a-guid");

            var command = CreateCommand();

            var act = () => _handler.Handle(
                command,
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

            var command = CreateCommand();

            var act = () => _handler.Handle(
                command,
                CancellationToken.None);

            var exception = await Assert.ThrowsAsync<NotFoundException>(act);

            Assert.Equal(
                "User not found.",
                exception.Message);

            await _azureBlobService
                .DidNotReceive()
                .UploadAsync(
                    Arg.Any<string>(),
                    Arg.Any<Stream>(),
                    Arg.Any<string>(),
                    Arg.Any<BlobContentCategoryEnum>(),
                    Arg.Any<bool>(),
                    Arg.Any<string?>());
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

        private static UploadProfilePictureCommand CreateCommand(
            string fileName = "profile.jpg")
        {
            var file = Substitute.For<IFormFile>();

            file.FileName.Returns(fileName);
            file.ContentType.Returns("image/jpeg");
            file.Length.Returns(1024);

            var stream = new MemoryStream([1, 2, 3]);

            file.OpenReadStream().Returns(stream);

            var request = new UploadProfilePictureRequest
            {
                File = file
            };

            return new UploadProfilePictureCommand(request);
        }

        private static CustomerEntity CreateCustomer(Guid id)
        {
            return new CustomerEntity
            {
                Id = id,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };
        }

        private static StaffEntity CreateStaff(Guid id)
        {
            return new StaffEntity
            {
                Id = id,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@example.com"
            };
        }
    }
}
