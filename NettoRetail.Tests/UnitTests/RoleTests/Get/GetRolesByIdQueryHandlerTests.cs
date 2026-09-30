using NettoRetail.Application.CQRS.Roles.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.RoleTests.Get;

public class GetRolesByIdQueryHandlerTests
{
    private readonly IRoleRepository _roleRepository;
    private readonly IShopRepository _shopRepository;
    private readonly GetRolesByIdQueryHandler _handler;

    public GetRolesByIdQueryHandlerTests()
    {
        _roleRepository = Substitute.For<IRoleRepository>();
        _shopRepository = Substitute.For<IShopRepository>();
        _handler = new GetRolesByIdQueryHandler(_roleRepository, _shopRepository);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenRoleDoesNotExist()
    {
        // Arrange
        int roleId = 99;
        int shopId = 1;
        var query = new GetRolesQueryById(roleId, shopId);

        _shopRepository
            .ExistsAsync(Arg.Any<Specification<ShopEntity>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _roleRepository
            .ExistsAsync(Arg.Any<Specification<RoleEntity>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnRoleResponse_WhenRoleExists()
    {
        // Arrange
        int roleId = 1;
        int shopId = 1;
        var query = new GetRolesQueryById(roleId, shopId);

        var existingRole = new RoleEntity
        {
            Id = roleId,
            Name = "Admin",
            Description = "Admin role",
            StaffMembers = new List<StaffEntity>()
        };

        ICollection<RoleEntity> roleList = new List<RoleEntity> { existingRole };

        _shopRepository
            .ExistsAsync(Arg.Any<Specification<ShopEntity>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _roleRepository
            .ExistsAsync(Arg.Any<Specification<RoleEntity>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _roleRepository
            .GetAllPaginatedAsync(Arg.Any<Specification<RoleEntity>>(), Arg.Any<CancellationToken>())
            .Returns((roleList, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingRole.Id, result.Id);
        Assert.Equal(existingRole.Name, result.Name);
    }
}