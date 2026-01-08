using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Role;
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
        var request = new CreateRoleRequest()
        {
            Name = "Account Officer",
            Description = "Handles financial responsibilities",
            PermissionIds = new[] { 1, 2 }
        };
        var cmd = new CreateRoleCommand(request, 10);
        _shopRepo.Exists(Arg.Any<Specification<ShopEntity>>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        _permissionRepo.GetIdsBySpecAsync(Arg.Any<Specification<PermissionEntity>>(), Arg.Any<CancellationToken>())
                       .Returns(await TaskList(1, 2));
        RoleEntity persisted = null!;
        _roleRepo.AddAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>()).Returns(ci =>
        {
            persisted = ci.Arg<RoleEntity>();
            persisted.Id = 777;
            return Task.FromResult(persisted);
        });
        var handler = new CreateRoleCommandHandler(_roleRepo, _permissionRepo, _shopRepo);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(777, result.Id);
        Assert.Equal("Account Officer", result.Name);
        Assert.Equal("Handles financial responsibilities", result.Description);
        Assert.Equal(10, result.ShopId);
        Assert.Equal(2, result.PermissionIds.Count);
        Assert.Contains(1, result.PermissionIds);
        Assert.Contains(2, result.PermissionIds);
        Assert.NotNull(persisted);
        Assert.Equal("Account Officer", persisted.Name);
        Assert.Equal("Handles financial responsibilities", persisted.Description);
        Assert.Equal(10, persisted.ShopId);
        Assert.Equal(2, persisted.PermissionRoles.Count);
        Assert.True(persisted.PermissionRoles.All(pr => pr.Role == persisted));
        Assert.True(persisted.PermissionRoles.Select(pr => pr.PermissionId).OrderBy(x => x).SequenceEqual(new[] { 1, 2 }));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenShopDoesNotExist()
    {
        // Arrange
        var request = new CreateRoleRequest()
        {
            Name = "Account Officer",
            Description = "Handles financial responsibilities",
            PermissionIds = new[] { 1, 2 }
        };

        var cmd = new CreateRoleCommand(request, 999);
        _shopRepo.Exists(Arg.Any<Specification<ShopEntity>>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));
        var handler = new CreateRoleCommandHandler(_roleRepo, _permissionRepo, _shopRepo);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenNoPermissionsProvided()
    {
        // Arrange
        var request = new CreateRoleRequest()
        {
            Name = "Account Officer",
            Description = "Handles financial responsibilities",
            PermissionIds = Array.Empty<int>()
        };

        var cmd = new CreateRoleCommand(request, 1);
        _shopRepo.Exists(Arg.Any<Specification<ShopEntity>>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        var handler = new CreateRoleCommandHandler(_roleRepo, _permissionRepo, _shopRepo);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenAnyPermissionIsInvalid()
    {
        // Arrange
        var request = new CreateRoleRequest()
        {
            Name = "Account Officer",
            Description = "Handles financial responsibilities",
            PermissionIds = new[] { 1, 2, 3 }
        };

        var cmd = new CreateRoleCommand(request, 1);
        _shopRepo.Exists(Arg.Any<Specification<ShopEntity>>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        _permissionRepo.GetIdsBySpecAsync(Arg.Any<Specification<PermissionEntity>>(), Arg.Any<CancellationToken>())
                       .Returns(await TaskList(1, 2));
        var handler = new CreateRoleCommandHandler(_roleRepo, _permissionRepo, _shopRepo);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(cmd, CancellationToken.None));
    }
}