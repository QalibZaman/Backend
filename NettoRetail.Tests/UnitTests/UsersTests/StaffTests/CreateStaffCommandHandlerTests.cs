using MapsterMapper;
using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Services.IStaffService;
using NettoRetail.Domain.Entities;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.UsersTests.StaffTests;

public class CreateStaffCommandHandlerTests
{
    private readonly IStaffService _staffService;
    private readonly IMapper _mapper;

    public CreateStaffCommandHandlerTests()
    {
        _staffService = Substitute.For<IStaffService>();
        _mapper = MapperFactory.Create();
    }

    [Fact]
    public async Task Handle_ShouldDelegateToStaffService_AndMapResponse()
    {
        // Arrange
        var request = new CreateStaffRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            RoleId = 2
        };
        var createdAccount = new StaffEntity
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            RoleId = request.RoleId,
            PasswordHash = "hash"
        };
        var command = new CreateStaffCommand(request, 3);
        var handler = new CreateStaffCommandHandler(_staffService, _mapper);

        _staffService
            .CreateStaffAsync(request, 3, Arg.Any<CancellationToken>())
            .Returns(createdAccount);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        await _staffService.Received(1)
            .CreateStaffAsync(request, 3, Arg.Any<CancellationToken>());
        Assert.Equal(createdAccount.Id, response.Id);
        Assert.Equal(createdAccount.Email, response.Email);
        Assert.Equal(createdAccount.RoleId, response.RoleId);
    }
}
