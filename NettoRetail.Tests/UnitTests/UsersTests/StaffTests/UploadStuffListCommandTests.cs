using Microsoft.AspNetCore.Http;
using FluentValidation;
using FluentValidation.Results;
using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services.IStaffService;
using NettoRetail.Shared.Constants;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.UsersTests.StuffTests;

public class UploadStuffListCommandHandlerTests
{
    private readonly IStaffService _staffServise;
    private readonly ICsvFileService _csvFileService;
    private readonly IValidator<List<StaffCsvModel>> _validator;

    public UploadStuffListCommandHandlerTests()
    {
        _staffServise = Substitute.For<IStaffService>();
        _csvFileService = Substitute.For<ICsvFileService>();
        _validator = Substitute.For<IValidator<List<StaffCsvModel>>>();
    }

    [Fact]
    public async Task Handle_ShouldThrowUploadStaffError_WhenFileIsNull()
    {
        // Arrange
        var command = new UploadStaffListCommand(null!, 1);
        var handler = new UploadStaffListCommandHandler(_csvFileService, _validator, _staffServise);
        _csvFileService
            .ReadStaffListAsync(null!, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<StaffCsvModel>>(new BadHttpRequestException(CsvUploadErrors.InvalidFileFormat)));

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidFileFormat_WhenFileExtentionIsWrong()
    {
        // Arrange
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns("Mockfile.ccv");
        file.Length.Returns(1);
        var command = new UploadStaffListCommand(file, 1);
        var handler = new UploadStaffListCommandHandler(_csvFileService, _validator, _staffServise);
        _csvFileService
            .ReadStaffListAsync(file, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<StaffCsvModel>>(new BadHttpRequestException(CsvUploadErrors.InvalidFileFormat)));

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldValidateAndCreateStaff_WhenFileIsValid()
    {
        // Arrange
        var file = Substitute.For<IFormFile>();
        var staff = new List<StaffCsvModel>
        {
            new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                Role = "1"
            }
        };
        var command = new UploadStaffListCommand(file, 1);
        var handler = new UploadStaffListCommandHandler(_csvFileService, _validator, _staffServise);

        _csvFileService
            .ReadStaffListAsync(file, Arg.Any<CancellationToken>())
            .Returns(staff);
        _validator
            .ValidateAsync(staff, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await _validator.Received(1).ValidateAsync(staff, Arg.Any<CancellationToken>());
        await _staffServise.Received(1).CreateStaffAsync(staff, 1, Arg.Any<CancellationToken>());
    }
}
