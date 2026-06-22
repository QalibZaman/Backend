using MapsterMapper;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Services.IStaffService;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.UsersTests.StaffTests;

public class UpdateStaffServiceTests
{
    private readonly IStaffRepository _accountRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly StaffService _staffService;
    private readonly IEmailBuilderService _emailBuilderService;
    private readonly IEmailSender _emailSender;
    private readonly ISessionTerminationService _sessionTerminationService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStaffServiceTests()
    {
        _accountRepository = Substitute.For<IStaffRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _emailBuilderService = Substitute.For<IEmailBuilderService>();
        _emailSender = Substitute.For<IEmailSender>();
        _sessionTerminationService = Substitute.For<ISessionTerminationService>();
        _mapper = MapperFactory.Create();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _staffService = new StaffService(
            _accountRepository,
            _roleRepository,
            _emailSender,
            _emailBuilderService,
            _sessionTerminationService,
            _mapper,
            _unitOfWork);

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

        _accountRepository.ExistsAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
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

        _accountRepository.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
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

        public static StaffEntity CreateStaffEntity(StaffStatusEnum status)
        {
            return new StaffEntity()
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
