using Microsoft.AspNetCore.Http;
using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services.IStaffService;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.UsersTests.StuffTests;

public class UploadStuffListCommandHandlerTests
{
    private readonly IStaffServise _staffServise;
    private readonly ICsvFileService _csvFileService;
    public UploadStuffListCommandHandlerTests()
    {
        _staffServise = Substitute.For<IStaffServise>();
        _csvFileService = Substitute.For<ICsvFileService>();
    }

    [Fact]
    public async Task Handle_ShouldThrownUploadStuffError_WhenFileIsNull()
    {
        //Arrange
        var command = new UploadStaffListCommand(null, 1);
        var handler = new UploadStaffListCommandHandler(_csvFileService, _staffServise);
        //Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(()=> handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrownInvalidFileFormat_WhenFileExtentionIsWrong()
    {
        //Arrange
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns("Mockfile.ccv");
        file.Length.Returns(1);
        var command = new UploadStaffListCommand(null, 1);
        var handler = new UploadStaffListCommandHandler(_csvFileService, _staffServise);
        //Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => handler.Handle(command, CancellationToken.None));
    }
}
