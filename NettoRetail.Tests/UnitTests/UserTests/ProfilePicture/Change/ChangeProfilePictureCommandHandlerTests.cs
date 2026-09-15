using Microsoft.AspNetCore.Http;
using NettoRetail.Application.CQRS.Users.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services.IImageValidatorService;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.UserTests.ProfilePicture.Change;

public class ChangeProfilePictureCommandHandlerTests
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IAzureBlobService _azureBlobService;
    private readonly IImageValidatorService _imageValidatorService;
    private readonly ChangeProfilePictureCommandHandler _handler;


    public ChangeProfilePictureCommandHandlerTests()
    {
        _customerRepository = Substitute.For<ICustomerRepository>();
        _staffRepository = Substitute.For<IStaffRepository>();
        _azureBlobService = Substitute.For<IAzureBlobService>();
        _imageValidatorService = Substitute.For<IImageValidatorService>();
        _handler = new ChangeProfilePictureCommandHandler(
            _customerRepository, _staffRepository, _azureBlobService, _imageValidatorService);
    }
    
    [Fact]
    public async Task Handler_ShouldThrowBadRequestException_WhenCustomerProfileImageIsNull()
    {
        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Test",
            Email = "test@gmail.com",
            PhoneNumber = "1122334455",
            RoleId = 1,
            ProfileImageUrl = null,
            Status = StaffStatusEnum.CreatePassword,
        };

        
        var request = new UploadProfilePictureRequest
        {
            File = Substitute.For<IFormFile>()
        };
        var command = new ChangeProfilePictureCommand(request, Guid.NewGuid());

        _customerRepository.GetWithRelatedDataAsync(Arg.Any<GetCustomerByIdSpecification>(), CancellationToken.None)
            .Returns(customer);

        var exception = await Assert.ThrowsAnyAsync<BadRequestException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });
        Assert.Equal(ValidationErrorMessages.User.ProfileImageDoesNotExistErrorMessage, exception.Message);

        await _staffRepository.DidNotReceive()
            .GetWithRelatedDataAsync(Arg.Any<GetStaffByIdSpecification>(), CancellationToken.None);
        await _customerRepository.DidNotReceive().UpdateAsync(Arg.Any<CustomerEntity>(), Arg.Any<CancellationToken>());
        await _staffRepository.DidNotReceive().UpdateAsync(Arg.Any<StaffEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handler_ShouldThrowBadRequestException_WhenStaffProfileImageIsNull()
    {
        var staff = new StaffEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Test",
            Email = "test@gmail.com",
            ProfileImageUrl = null,
        };
        
        var request = new UploadProfilePictureRequest
        {
            File = Substitute.For<IFormFile>()
        };
        var command = new ChangeProfilePictureCommand(request, Guid.NewGuid());

        _staffRepository.GetWithRelatedDataAsync(Arg.Any<GetStaffByIdSpecification>(), CancellationToken.None)
            .Returns(staff);

        var exception = await Assert.ThrowsAnyAsync<BadRequestException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });
        Assert.Equal(ValidationErrorMessages.User.ProfileImageDoesNotExistErrorMessage, exception.Message);

        _customerRepository.GetWithRelatedDataAsync(Arg.Any<GetCustomerByIdSpecification>(), CancellationToken.None)
            .Returns((Task<CustomerEntity>)null);
        await _staffRepository.DidNotReceive().UpdateAsync(Arg.Any<StaffEntity>(), Arg.Any<CancellationToken>());
        await _customerRepository.DidNotReceive().UpdateAsync(Arg.Any<CustomerEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handler_ShouldThrowNotFoundException_WhenUserNotFound()
    {
        
        var request = new UploadProfilePictureRequest
        {
            File = Substitute.For<IFormFile>()
        };
        var command = new ChangeProfilePictureCommand(request, Guid.NewGuid());

        _customerRepository.GetWithRelatedDataAsync(Arg.Any<GetCustomerByIdSpecification>(), CancellationToken.None)
            .Returns(Task.FromResult<CustomerEntity?>(null));
        _staffRepository.GetWithRelatedDataAsync(Arg.Any<GetStaffByIdSpecification>(), CancellationToken.None)
            .Returns(Task.FromResult<StaffEntity?>(null));

        var exception = await Assert.ThrowsAnyAsync<NotFoundException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });
        Assert.Equal(ValidationErrorMessages.User.UserNotFoundErrorMessage, exception.Message);
    }

    [Fact]
    public async Task Handler_ShouldUpdateCustomerProfilePicture_WhenProfileImageIsNotNull()
    {
        var customer = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Test",
            Email = "test@gmail.com",
            PhoneNumber = "1122334455",
            RoleId = 1,
            ProfileImageUrl = "profile-image-url",
            Status = StaffStatusEnum.CreatePassword,
        };

        var file = Substitute.For<IFormFile>();

        file.FileName.Returns("profile.jpg");
        file.ContentType.Returns("image/jpeg");
        file.OpenReadStream().Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

        var request = new UploadProfilePictureRequest
        {
            File = file
        };

        var command = new ChangeProfilePictureCommand(
            request,
            customer.Id);

        _customerRepository
            .GetWithRelatedDataAsync(
                Arg.Any<GetCustomerByIdSpecification>(),
                CancellationToken.None)
            .Returns(customer);

        _azureBlobService
            .UploadAsync(
                Arg.Any<string>(),
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                Arg.Any<BlobContentCategoryEnum>(),
                Arg.Any<bool>(),
                Arg.Any<string>())
            .Returns("new-profile-image-url");

        _azureBlobService
            .DeleteByUrlAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        await _customerRepository.Received(1).UpdateAsync(
            Arg.Is<CustomerEntity>(x =>
                x.Id == customer.Id &&
                x.ProfileImageUrl == "new-profile-image-url"),
            Arg.Any<CancellationToken>());

        await _staffRepository.DidNotReceive()
            .UpdateAsync(
                Arg.Any<StaffEntity>(),
                Arg.Any<CancellationToken>());

        await _staffRepository.DidNotReceive()
            .GetWithRelatedDataAsync(
                Arg.Any<GetStaffByIdSpecification>(),
                CancellationToken.None);

        await _azureBlobService.Received(1)
            .DeleteByUrlAsync(
                "profile-image-url",
                Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handler_ShouldUpdateStaffProfilePicture_WhenProfileImageIsNotNull()
    {
        var staff = new StaffEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Test",
            Email = "test@gmail.com",
            ProfileImageUrl = "profile-image-url"
        };

        var file = Substitute.For<IFormFile>();

        file.FileName.Returns("profile.jpg");
        file.ContentType.Returns("image/jpeg");
        file.OpenReadStream().Returns(new MemoryStream(new byte[] { 1, 2, 3 }));

        var request = new UploadProfilePictureRequest
        {
            File = file
        };

        var command = new ChangeProfilePictureCommand(
            request,
            staff.Id);

        _customerRepository
            .GetWithRelatedDataAsync(
                Arg.Any<GetCustomerByIdSpecification>(),
                CancellationToken.None)
            .Returns(Task.FromResult<CustomerEntity?>(null));
        _staffRepository
            .GetWithRelatedDataAsync(
                Arg.Any<GetStaffByIdSpecification>(),
                CancellationToken.None)
            .Returns(staff);

        _azureBlobService
            .UploadAsync(
                Arg.Any<string>(),
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                Arg.Any<BlobContentCategoryEnum>(),
                Arg.Any<bool>(),
                Arg.Any<string>())
            .Returns("new-profile-image-url");

        _azureBlobService
            .DeleteByUrlAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        await _staffRepository.Received(1).UpdateAsync(
            Arg.Is<StaffEntity>(x =>
                x.Id == staff.Id &&
                x.ProfileImageUrl == "new-profile-image-url"),
            Arg.Any<CancellationToken>());

        await _customerRepository.DidNotReceive()
            .UpdateAsync(
                Arg.Any<CustomerEntity>(),
                Arg.Any<CancellationToken>());

        await _azureBlobService.Received(1)
            .DeleteByUrlAsync(
                "profile-image-url",
                Arg.Any<CancellationToken>());
    }
}