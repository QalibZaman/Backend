using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;
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
    public async Task Handle_ShouldThrowNotFoundException_WhenRoleDoesNotExist()
    {
        // Arrange
        int roleId = 99;
        int shopId = 1;
        var command = new DeleteRoleCommand(roleId, shopId);

        _roleRepository
            .GetAsync(Arg.Any<Specification<RoleEntity>>(), Arg.Any<CancellationToken>())
            .Returns((RoleEntity?)null);

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Role with provided id and shopId not found", exception.Message);

        await _roleRepository
            .DidNotReceive()
            .DeleteAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldDeleteRole_WhenRoleExists()
    {
        // Arrange
        int roleId = 1;
        int shopId = 1; 
        var command = new DeleteRoleCommand(roleId, shopId);
        var existingRole = new RoleEntity { Id = roleId, Name = "Manager" };

        _roleRepository
            .GetAsync(Arg.Any<Specification<RoleEntity>>(), Arg.Any<CancellationToken>())
            .Returns(existingRole);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _roleRepository
            .Received(1)
            .DeleteAsync(existingRole, Arg.Any<CancellationToken>());
    }
}