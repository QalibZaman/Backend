using Microsoft.AspNetCore.Http;
using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Interfaces;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.UsersTests.StuffTests;

public class UploadStuffListCommandHandlerTests
{
    private readonly IAccountRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IEmailSender _emailSender;
    private readonly IEmailBuilderService _emailBuilderService;
    private readonly ICsvFileService _csvFileService;
    public UploadStuffListCommandHandlerTests()
    {
        _userRepository = Substitute.For<IAccountRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _emailSender = Substitute.For<IEmailSender>();
        _emailBuilderService = Substitute.For<IEmailBuilderService>();
        _csvFileService = Substitute.For<ICsvFileService>();
    }

    [Fact]
    public async Task Handle_ShouldThrownUploadStuffError_WhenFileIsNull()
    {
        //Arrange
        var command = new UploadStaffListCommand(null, 1);
        var handler = new UploadStaffListCommandHandler(_userRepository, _roleRepository, _emailSender,
            _emailBuilderService, _csvFileService);
        //Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(()=> handler.Handle(command, CancellationToken.None));
    }
}
