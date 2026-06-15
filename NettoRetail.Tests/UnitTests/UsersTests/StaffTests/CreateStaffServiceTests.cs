using MapsterMapper;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Services.IStaffService;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Tests.Helpers;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Data;

namespace NettoRetail.Tests.UnitTests.UsersTests.StaffTests;

public class CreateStaffServiceTests
{
    private readonly IStaffRepository _accountRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly StaffService _staffService;
    private readonly IEmailBuilderService _emailBuilderService;
    private readonly IEmailSender _emailSender;
    private readonly ISessionTerminationService _sessionTerminationService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStaffServiceTests()
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
    public async Task CreateStaffAsync_CsvModel_ShouldThrowBadRequest_WhenRoleFormatIsInvalid()
    {
        var csvModel = CreateStaffCsvModel("invalid_role");

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _staffService.CreateStaffAsync(csvModel, 1, CancellationToken.None));

        _unitOfWork.DidNotReceive().BeginTransaction();
    }

    [Fact]
    public async Task CreateStaffAsync_Request_ShouldThrowNotFound_WhenRoleDoesNotExist()
    {
        var request = CreateStaffRequest();
        _roleRepository.GetAsync(Arg.Any<GetRoleByShopIdOrAdminIdSpecification>(), Arg.Any<CancellationToken>())
            .ReturnsNull();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _staffService.CreateStaffAsync(request, 1, CancellationToken.None));

        _unitOfWork.DidNotReceive().BeginTransaction();
    }

    [Fact]
    public async Task CreateStaffAsync_Request_ShouldThrowConflict_WhenEmailIsUsed()
    {
        var request = CreateStaffRequest();
        var role = CreateRoleEntity();

        _roleRepository.GetAsync(Arg.Any<GetRoleByShopIdOrAdminIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(role);

        _accountRepository.ExistsAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _staffService.CreateStaffAsync(request, 1, CancellationToken.None));

        _unitOfWork.DidNotReceive().BeginTransaction();
    }

    [Fact]
    public async Task CreateStaffAsync_Request_ShouldCreateStaff_WhenDataIsValid()
    {
        var request = CreateStaffRequest();
        var role = CreateRoleEntity();

        var mockTransaction = Substitute.For<IDbTransaction>();
        _unitOfWork.BeginTransaction().Returns(mockTransaction);

        _roleRepository.GetAsync(Arg.Any<GetRoleByShopIdOrAdminIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(role);

        _accountRepository.ExistsAsync(Arg.Any<GetStaffByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var createdStaff = await _staffService.CreateStaffAsync(request, 1, CancellationToken.None);

        await _accountRepository.Received(1).AddAsync(Arg.Is<StaffEntity>(s => s.Email == request.Email), Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendAsync(Arg.Any<EmailModel>(), Arg.Any<Guid>());
        Assert.Equal(StaffStatusEnum.CreatePassword, createdStaff.Status);

        _unitOfWork.Received(1).BeginTransaction();
        mockTransaction.Received(1).Commit();
    }

    [Fact]
    public async Task CreateStaffAsync_BulkCsv_ShouldReturn_WhenCollectionIsEmpty()
    {
        var emptyList = new List<StaffCsvModel>();

        await _staffService.CreateStaffAsync(emptyList, 1, CancellationToken.None);

        await _accountRepository.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<StaffEntity>>(), Arg.Any<CancellationToken>());
        _unitOfWork.DidNotReceive().BeginTransaction();
    }

    [Fact]
    public async Task CreateStaffAsync_BulkCsv_ShouldThrowBadRequest_WhenEmailAlreadyExists()
    {
        var csvModels = new List<StaffCsvModel> { CreateStaffCsvModel("1") };
        var existingStaff = CreateStaffEntity(StaffStatusEnum.Active);

        _accountRepository.GetAllBySpecAsync(Arg.Any<GetStaffByEmailsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<StaffEntity> { existingStaff });

        _roleRepository.GetAllBySpecAsync(Arg.Any<GetRolesByIdsAndShopIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<RoleEntity> { CreateRoleEntity() });

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _staffService.CreateStaffAsync(csvModels, 1, CancellationToken.None));

        _unitOfWork.DidNotReceive().BeginTransaction();
    }

    private CreateStaffRequest CreateStaffRequest()
    {
        return new CreateStaffRequest
        {
            FirstName = "NewName",
            LastName = "NewLastname",
            Email = "newemail@gmail.com",
            RoleId = 1
        };
    }

    private StaffCsvModel CreateStaffCsvModel(string role)
    {
        return new StaffCsvModel
        {
            FirstName = "CsvName",
            LastName = "CsvLastname",
            Email = "email@gmail.com",
            Role = role
        };
    }

    private StaffEntity CreateStaffEntity(StaffStatusEnum status)
    {
        return new StaffEntity
        {
            Id = Guid.NewGuid(),
            FirstName = "Name",
            LastName = "Lastname",
            Email = "email@gmail.com",
            PasswordHash = "passwordHash@",
            ShopId = 1,
            Status = status,
            PasswordUpdatedAt = DateTime.UtcNow
        };
    }

    private RoleEntity CreateRoleEntity()
    {
        return new RoleEntity
        {
            Id = 1,
            Name = "Admin",
            ShopId = 1
        };
    }
    
}
