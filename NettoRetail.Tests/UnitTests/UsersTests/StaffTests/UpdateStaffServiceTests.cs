using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Services.IStaffService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.UsersTests.StaffTests;

public class UpdateStaffServiceTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly StaffService _staffService;
    private readonly IEmailBuilderService _emailBuilderService;
    private readonly IEmailSender _emailSender;
    private readonly ISessionTerminationService _sessionTerminationService;

    public UpdateStaffServiceTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _emailBuilderService = Substitute.For<IEmailBuilderService>();
        _emailSender = Substitute.For<IEmailSender>();
        _sessionTerminationService = Substitute.For<ISessionTerminationService>();
        _staffService = new StaffService(_accountRepository, _roleRepository, _emailSender, _emailBuilderService, _sessionTerminationService);

    }

    [Fact]
    public async Task StaffService_ShouldThrowBadRequest_WhenStatusIsDeactivated()
    {
        //Arrange
        var request = CreateStub.CreateUpdateRequest(false);
        var staff = CreateStub.CreateStaffEntity(StaffStatusEnum.Deactivated);

        var userId = Guid.NewGuid();

        //Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => 
        _staffService.UpdateStaffAsync(request, staff, CancellationToken.None));
    }

    [Fact]
    public async Task StaffService_ShouldThrowConflict_WhenEmailIsUsed()
    {
        //Arrange
        var request = CreateStub.CreateUpdateRequest(true);
        var staff = CreateStub.CreateStaffEntity(StaffStatusEnum.Active);

        var userId = Guid.NewGuid();

        _accountRepository.Exists(Arg.Any<GetAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
        _staffService.UpdateStaffAsync(request, staff, CancellationToken.None));
    }

    [Fact]
    public async Task StaffService_ShouldUpdateStaff_WhenRequestIsValid()
    {
        //Arrange
        var request = CreateStub.CreateUpdateRequest(true);
        var staff = CreateStub.CreateStaffEntity(StaffStatusEnum.Active);

        var userId = Guid.NewGuid();

        _accountRepository.GetAsync(Arg.Any<GetAccountByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(staff);

        //Act
        await _staffService.UpdateStaffAsync(request, staff, CancellationToken.None);

        //Assert

        Assert.Equal(staff.FirstName, request.FirstName);
    }

    private static class CreateStub
    {
        public static UpdateStaffRequest CreateUpdateRequest(bool isActive)
        {
            return new UpdateStaffRequest()
            {
                FirstName = "Test",
                LastName = "Test",
                Email = "email2@gmail.com",
                RoleId = 1,
                IsActive = isActive,
            };
        }

        public static AccountEntity CreateStaffEntity(StaffStatusEnum status)
        {
            return new AccountEntity()
            {
                FirstName = "Name",
                LastName = "Lastname",
                Email = "email@gmail.com",
                PasswordHash = "passwordHash@",
                ShopId = 1,
                Status = status,
                PasswordUpdatedAt = DateTime.UtcNow,
            };
        }
    }
}
