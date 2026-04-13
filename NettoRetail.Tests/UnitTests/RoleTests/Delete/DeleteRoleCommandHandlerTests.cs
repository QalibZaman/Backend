using FluentAssertions;
using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.RoleTests.Delete;

public class DeleteRoleCommandHandlerTests
{
    private readonly IRoleRepository _roleRepository;
    private readonly DeleteRoleCommandHandler _handler;
    public DeleteRoleCommandHandlerTests()
    {
        _roleRepository = Substitute.For<IRoleRepository>();
        _handler = new DeleteRoleCommandHandler(_roleRepository);
    }

    [Fact]
    public async Task Handle_ShouldDeleteRole_WhenRoleExists()
    {
        //Arrange
        var command = new DeleteRoleCommand(2, 1);
        var role = new RoleEntity { Id = command.Id, ShopId = command.ShopId, Name = "Test Role" };
        _roleRepository.GetAsync(Arg.Any<GetRoleByIdAndShopIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(role);

        //Act
        await _handler.Handle(command, CancellationToken.None);

        //Assert
        await _roleRepository.Received(1).DeleteAsync(role, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenRoleDoesNotExist()
    {
        //Arrange
        var command = new DeleteRoleCommand(2, 1);
        _roleRepository.GetAsync(Arg.Any<GetRoleByIdAndShopIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((RoleEntity?)null);

        //Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(HandlerConstants.Role.RoleNotFoundMessage);
    }
}
