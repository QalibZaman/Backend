using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.RoleHandlerTests;

public class CreateRoleCommandHandlerTests
{
    private readonly IRoleRepository _roleRepo = Substitute.For<IRoleRepository>();
    private readonly IPermissionRepository _permissionRepo = Substitute.For<IPermissionRepository>();
    private readonly IShopRepository _shopRepo = Substitute.For<IShopRepository>();

    private static Task<IReadOnlyList<int>> TaskList(params int[] ids) =>
        Task.FromResult<IReadOnlyList<int>>(ids.ToList());

    [Fact]
    public async Task Handle_ShouldCreateRole_WhenShopExists_AndPermissionsValid()
    {
        // Arrange
        var cmd = new CreateRoleCommand(
            Name: "  Account Officer  ",                // should be trimmed
            Description: "  Handles financial responsibilities  ",                  // should be trimmed
            ShopId: 10,
            PermissionIds: new[] { 1, 1, 2 }            // duplicates should be removed
        );

        _shopRepo.Exists(Arg.Any<Specification<ShopEntity>>(), Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult(true));

        _permissionRepo.GetIdsBySpecAsync(Arg.Any<Specification<PermissionEntity>>(), Arg.Any<CancellationToken>())
                       .Returns((IEnumerable<int>)TaskList(1, 2));        // both valid

        RoleEntity captured = null!;
        _roleRepo.AddAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>())
                 .Returns(ci =>
                 {
                     captured = ci.Arg<RoleEntity>();
                     captured.Id = 777;                // simulate DB-generated Id
                     return Task.FromResult(captured);
                 });

        var handler = new CreateRoleCommandHandler(_roleRepo, _permissionRepo, _shopRepo);

        // Act
        var response = await handler.Handle(cmd, CancellationToken.None);

        // Assert (response)
        Assert.NotNull(response);
        Assert.Equal(777, response.Id);
        Assert.Equal("Account Officer", response.Name);
        Assert.Equal("Handles financial responsibilities", response.Description);
        Assert.Equal(10, response.ShopId);
        Assert.Equal(2, response.PermissionIds.Count);
        Assert.Contains(1, response.PermissionIds);
        Assert.Contains(2, response.PermissionIds);

        // Assert (entity persisted)
        Assert.NotNull(captured);
        Assert.Equal("Account Officer", captured.Name);
        Assert.Equal("Handles financial responsibilities", captured.Description);
        Assert.Equal(10, captured.ShopId);
        Assert.Equal(2, captured.PermissionRoles.Count);
        Assert.True(captured.PermissionRoles.All(pr => pr.Role == captured));
        Assert.True(captured.PermissionRoles.Select(pr => pr.PermissionId).OrderBy(x => x).SequenceEqual(new[] { 1, 2 }));
    }

    [Fact]
    public async Task Handle_ShouldNullDescription_WhenWhitespaceOnly()
    {
        // Arrange
        var cmd = new CreateRoleCommand(
            Name: "Name",
            Description: "   ",  // whitespace => null
            ShopId: 1,
            PermissionIds: new[] { 5 }
        );

        _shopRepo.Exists(Arg.Any<Specification<ShopEntity>>(), Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult(true));

        _permissionRepo.GetIdsBySpecAsync(Arg.Any<Specification<PermissionEntity>>(), Arg.Any<CancellationToken>())
                       .Returns((IEnumerable<int>)TaskList(5));

        RoleEntity saved = null!;
        _roleRepo.AddAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>())
                 .Returns(ci =>
                 {
                     saved = ci.Arg<RoleEntity>();
                     saved.Id = 1;
                     return Task.FromResult(saved);
                 });

        var handler = new CreateRoleCommandHandler(_roleRepo, _permissionRepo, _shopRepo);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(saved.Description);
        Assert.Null(result.Description);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenShopDoesNotExist()
    {
        // Arrange
        var cmd = new CreateRoleCommand("Name", "Desc", 999, new[] { 1, 2 });

        _shopRepo.Exists(Arg.Any<Specification<ShopEntity>>(), Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult(false));

        var handler = new CreateRoleCommandHandler(_roleRepo, _permissionRepo, _shopRepo);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenNoPermissionsProvided()
    {
        // Arrange
        var cmd = new CreateRoleCommand("Name", "Desc", 1, Array.Empty<int>());

        _shopRepo.Exists(Arg.Any<Specification<ShopEntity>>(), Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult(true));

        var handler = new CreateRoleCommandHandler(_roleRepo, _permissionRepo, _shopRepo);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenAnyPermissionIsInvalid()
    {
        // Arrange: incoming [1,2,3], repo validates only [1,2]
        var cmd = new CreateRoleCommand("Name", "Desc", 1, new[] { 1, 2, 3 });

        _shopRepo.Exists(Arg.Any<Specification<ShopEntity>>(), Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult(true));

        _permissionRepo.GetIdsBySpecAsync(Arg.Any<Specification<PermissionEntity>>(), Arg.Any<CancellationToken>())
                       .Returns((IEnumerable<int>)TaskList(1, 2)); // 3 is invalid

        var handler = new CreateRoleCommandHandler(_roleRepo, _permissionRepo, _shopRepo);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(cmd, CancellationToken.None));
    }
}

