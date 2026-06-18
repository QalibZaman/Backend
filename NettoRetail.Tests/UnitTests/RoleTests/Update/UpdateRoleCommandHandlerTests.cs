using FluentAssertions;
using MapsterMapper;
using MediatR;
using NettoRetail.Application.CQRS.Roles.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.RoleTests.Update;

public class RoleUpdateCommandHandlerTests
{
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;
    private readonly RoleUpdateCommandHandler _handler;

    public RoleUpdateCommandHandlerTests()
    {
        _roleRepository = Substitute.For<IRoleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new RoleUpdateCommandHandler(_roleRepository, _mapper);
    }

    [Fact] 
    public async Task Handle_ShouldThrowConflictException_WhenIdIsAdmin()
    {
        int id = RoleConstants.Admin;
        int shopId = 10;
        var request = new UpdateRoleRequest
        {
            Name = "Test",
            Description = "Test Description",
            PermissionIds =
            [
                1,2,3,4,5
            ]
        };
        var command = new RoleUpdateCommand(id, shopId, request);
        var exception = await Assert.ThrowsAnyAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal(ValidationErrorMessages.Role.CannotEditAdminRoleErrorMessage, exception.Message);
        await _roleRepository.DidNotReceive().UpdateAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenIdIsNotFound()
    {
        int id = 3;
        int shopId = 10;
        var request = new UpdateRoleRequest
        {
            Name = "Test",
            Description = "Test Description",
            PermissionIds =
            [
                1,2,3,4,5
            ]
        };
        RoleEntity expectedResponse = null;
        var command = new RoleUpdateCommand(id, shopId, request);
        _roleRepository.GetAsync(Arg.Any<Specification<RoleEntity>>(), CancellationToken.None).Returns(expectedResponse);
        var exception = await Assert.ThrowsAnyAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal(ValidationErrorMessages.Role.RoleNotFoundErrorMessage, exception.Message);
        await _roleRepository.DidNotReceive().UpdateAsync(Arg.Any<RoleEntity>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_ShouldReturnUnit_WhenSuccess()
    {
        var id = 3;
        var shopId = 10;
        var request = new UpdateRoleRequest
        {
            Name = "Test",
            Description = "Test Description",
            PermissionIds =
            [
                1,2,3,4,5
            ]
        };
        var permissions = new List<PermissionRoleEntity>()
        {
            new ()
            {
                RoleId = id,
                PermissionId = 1,
            }
        };

        var expectedResponse = new RoleEntity
        {
            Id = id,
            ShopId = shopId,
            Name = "Test",
            Description = "Test Description",
            PermissionRoles =  permissions
        };
        var command = new RoleUpdateCommand(id, shopId, request);
        _roleRepository.GetAsync(Arg.Any<Specification<RoleEntity>>(), CancellationToken.None).Returns(expectedResponse);
        var response = await _handler.Handle(command, CancellationToken.None);
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(Unit.Value);


    }
}