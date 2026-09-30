using NettoRetail.Application.CQRS.Roles.Queries;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.RoleTests.Get;

public class GetRolesByNameQueryHandlerTests
{
    private readonly IRoleRepository _roleRepository;
    private readonly GetRolesByNameQueryHandler _handler;

    public GetRolesByNameQueryHandlerTests()
    {
        _roleRepository = Substitute.For<IRoleRepository>();
        _handler = new GetRolesByNameQueryHandler(_roleRepository);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenRoleDoesNotExist()
    {
        // Arrange
        string roleName = "NonExistent";
        var query = new GetRolesQueryByName(roleName);

        _roleRepository
            .GetAllPaginatedAsync(Arg.Any<Specification<RoleEntity>>(), Arg.Any<CancellationToken>())
            .Returns((new List<RoleEntity>(), 0));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnRoleResponse_WhenRoleExists()
    {
        // Arrange
        string roleName = "Admin";
        var query = new GetRolesQueryByName(roleName);
        var existingRole = new RoleEntity 
        { 
            Id = 1, 
            Name = roleName,
            Description = "Admin role"
        };

        _roleRepository
            .GetAllPaginatedAsync(Arg.Any<Specification<RoleEntity>>(), Arg.Any<CancellationToken>())
            .Returns((new List<RoleEntity> { existingRole }, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingRole.Id, result.Id);
        Assert.Equal(existingRole.Name, result.Name);
    }
}