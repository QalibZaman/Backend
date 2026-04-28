using Microsoft.Azure.Cosmos;
using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Services.IStaffService;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications;
using NSubstitute;
using System.Data;
using static NettoRetail.Shared.Constants.ValidationErrorMessages;

namespace NettoRetail.Tests.UnitTests.UsersTests.StaffTests;

public class UpdateStaffCommandHandlerTests
{
    private readonly IStaffService _staffService;
    private readonly IStaffRepository _staffRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateStaffCommandHandler _handler;
    private readonly IDbTransaction _transaction;

    public UpdateStaffCommandHandlerTests()
    {
        _staffService = Substitute.For<IStaffService>();
        _staffRepository = Substitute.For<IStaffRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _transaction = _unitOfWork.BeginTransaction();

        _unitOfWork.BeginTransaction().Returns(_transaction);

        _handler = new UpdateStaffCommandHandler(_staffRepository, _roleRepository, _staffService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDataIsValid()
    {
        //Arrange
        var staffId = Guid.NewGuid();
        var roleId = 10;
        var shopid = 1;
        var staff = BuildDefaultStaff(shopid);
        staff.Id = staffId;
        var role = new RoleEntity { Id = roleId, Name = "Role", ShopId = staff.ShopId };

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(staff);

        _roleRepository
            .Exists(Arg.Any<GetRoleByShopIdOrAdminIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = BuildCommand(staffId, shopid, roleId);

        //Act
        await _handler.Handle(command, CancellationToken.None);

        //Assert
        await _staffRepository.Received(1).UpdateAsync(Arg.Is<StaffEntity>(s =>
            s.Id == staffId), Arg.Any<CancellationToken>());

        _transaction.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenRoleDoesNotExist()
    {
        // Arrange
        var staffFromAnotherShop = BuildDefaultStaff();

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(staffFromAnotherShop);

        _roleRepository
            .Exists(Arg.Any<GetRoleByShopIdOrAdminIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var act = () => _handler.Handle(BuildCommand(shopId: 1, roleId: 99), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
        _transaction.DidNotReceive().Commit();
    }

    [Fact]
    public async Task Handle_ShouldThrowForbidden_WhenStaffIsFromAnotherShop()
    {
        // Arrange
        var staffFromAnotherShop = BuildDefaultStaff(shopId: 2);

        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(staffFromAnotherShop);

        // Act
        var act = () => _handler.Handle(BuildCommand(shopId: 1), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ForbiddenException>(act);
        _transaction.DidNotReceive().Commit();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenStaffDoesNotExist()
    {
        // Arrange
        _staffRepository
            .GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((StaffEntity?)null);

        // Act
        var act = () => _handler.Handle(BuildCommand(), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
        _transaction.DidNotReceive().Commit();
    }
    private UpdateStaffCommand BuildCommand(Guid? id = null, int shopId = 1, int? roleId = null)
    {
        var staffId = id ?? Guid.NewGuid();
        var request = new UpdateStaffRequest { RoleId = roleId };

        return new UpdateStaffCommand(staffId, request, shopId);
    }

    private StaffEntity BuildDefaultStaff(int shopId = 1)
    {
        return new StaffEntity
        {
            FirstName = "Name",
            LastName = "Lastname",
            Email = "email@gmail.com",
            PasswordHash = "passwordHash@",
            ShopId = shopId
        };
    }
}
