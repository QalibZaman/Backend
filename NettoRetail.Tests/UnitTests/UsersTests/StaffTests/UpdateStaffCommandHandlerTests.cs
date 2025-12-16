using Microsoft.Azure.Cosmos;
using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Services.IStaffService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;
using StackExchange.Redis;
using System.Data;

namespace NettoRetail.Tests.UnitTests.UsersTests.StaffTests;

public class UpdateStaffCommandHandlerTests
{
    private readonly IStaffService _staffService;
    private readonly IAccountRepository _accountRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStaffCommandHandlerTests()
    {
        _staffService = Substitute.For<IStaffService>();
        _accountRepository = Substitute.For<IAccountRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenRoleDoesNotExist()
    {
        var request = new UpdateStaffRequest()
        {
            RoleId = 1,
        };

        var user = new AccountEntity()
        {
            FirstName = "Name",
            LastName = "Lastname",
            Email = "email@gmail.com",
            PasswordHash = "passwordHash@",
            ShopId = 1
        };

        var id = Guid.NewGuid();
        var shopId = 1;

        var handler = new UpdateStaffCommandHandler(_accountRepository, _roleRepository, _staffService, _unitOfWork);
        var command = new UpdateStaffCommand(id, request, shopId);

        _accountRepository.GetAsync(Arg.Any<GetAccountByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _roleRepository.Exists(Arg.Any<GetRoleByShopIdOrAdminIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenStaffIsFromAnotherShop()
    {
        var request = new UpdateStaffRequest();

        var id = Guid.NewGuid();
        var shopId = 1;
        var user = new AccountEntity()
        {
            FirstName = "Name",
            LastName = "Lastname",
            Email = "email@gmail.com",
            PasswordHash = "passwordHash@",
            ShopId = 2
        };

        var handler = new UpdateStaffCommandHandler(_accountRepository, _roleRepository, _staffService, _unitOfWork);
        var command = new UpdateStaffCommand(id, request, shopId);

        _accountRepository.GetAsync(Arg.Any<GetAccountByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(user);

        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenStaffDoesNotExist()
    {
        var request = new UpdateStaffRequest();

        var id = Guid.NewGuid();
        var shopId = 1;

        var handler = new UpdateStaffCommandHandler(_accountRepository, _roleRepository, _staffService, _unitOfWork);
        var command = new UpdateStaffCommand(id, request, shopId);

        _accountRepository.GetAsync(Arg.Any<GetAccountByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<AccountEntity?>(null));

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}
