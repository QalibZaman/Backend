using FluentAssertions;
using MediatR;
using NettoRetail.Application.CQRS.Customers.Commands;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.Application.Services.IPasswordService;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.CustomerManagementTests;

public class ChangeCustomerPasswordCommandHandlerTests
{
    private readonly IPasswordService _passwordService;
    private readonly ChangeCustomerPasswordCommandHandler _handler;

    public ChangeCustomerPasswordCommandHandlerTests()
    {
        _passwordService = Substitute.For<IPasswordService>();
        _handler = new ChangeCustomerPasswordCommandHandler(_passwordService);
    }

    [Fact]
    public async Task Handle_ShouldCallPasswordServiceAndReturnUnit_WhenRequestIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest("OldOne123!", "NewOne123!", "NewOne123!");
        var command = new ChangeCustomerPasswordCommand(userId, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        await _passwordService.Received(1).ChangeCustomerPasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword,
            Arg.Any<CancellationToken>());
    }
}
