using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.RoleTests.Create;

public class CreateRoleCommandHandlerTests
{
    private readonly IRoleRepository _roleRepo = Substitute.For<IRoleRepository>();

    [Fact]
    public async Task Handle_ShouldCreateRole_WhenShopExists_AndPermissionsValid()
    {
        // Arrange

        string name = "Account Officer";
        string description = "Handles financial responsibilities";

        var request = new CreateRoleRequest()
        {
            Name = name,
            Description = description,
            PermissionIds = new[] { 1, 2 }
        };
        var cmd = new CreateRoleCommand(request, 10);

        RoleEntity persisted = null!;
        _roleRepo.AddAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>()).Returns(ci =>
        {
            persisted = ci.Arg<RoleEntity>();
            persisted.Id = 777;
            return Task.FromResult(persisted);
        });
        var handler = new CreateRoleCommandHandler(_roleRepo);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(777, result.Id);
        Assert.Equal(name, result.Name);
        Assert.Equal(10, result.ShopId);

        Assert.NotNull(persisted);
        Assert.Equal(name, persisted.Name);
        Assert.Equal(description, persisted.Description);
        Assert.Equal(10, persisted.ShopId);
        Assert.Equal(2, persisted.PermissionRoles.Count);

        Assert.True(persisted.PermissionRoles.All(pr => pr.Role == persisted));
        Assert.True(persisted.PermissionRoles.Select(pr => pr.PermissionId).OrderBy(x => x).SequenceEqual(new[] { 1, 2 }));

        await _roleRepo
            .Received(1)
            .AddAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>());
    }
}