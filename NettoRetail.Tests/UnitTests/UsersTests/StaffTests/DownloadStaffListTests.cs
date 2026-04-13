using MapsterMapper;
using Microsoft.Extensions.Logging;
using NettoRetail.Application.CQRS.Users.Queries.Staff;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Services.IStaffService;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.UsersTests.StaffTests;

public class DownloadStaffListTests
{
    private readonly IStaffRepository _staffRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IEmailSender _emailSender;
    private readonly IEmailBuilderService _emailBuilderService;
    private readonly ISessionTerminationService _sessionTerminationService;
    private readonly IMapper _mapper;
    private readonly ICsvFileService _csvFileService;
    private readonly ILogger<StaffService> _logger;
    private readonly StaffService _staffService;

    public DownloadStaffListTests()
    {
        _staffRepository = Substitute.For<IStaffRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _emailSender = Substitute.For<IEmailSender>();
        _emailBuilderService = Substitute.For<IEmailBuilderService>();
        _sessionTerminationService = Substitute.For<ISessionTerminationService>();
        _mapper = MapperFactory.Create();
        _csvFileService = Substitute.For<ICsvFileService>();
        _logger = Substitute.For<ILogger<StaffService>>();

        _staffService = new StaffService(
            _staffRepository,
            _roleRepository,
            _emailSender,
            _emailBuilderService,
            _sessionTerminationService,
            _mapper,
            _csvFileService,
            _logger);
    }

    [Fact]
    public async Task DownloadStaffCsvAsync_ShouldThrowBadRequest_WhenFilteredListIsEmpty()
    {
        _staffRepository.GetStaffAccounts(Arg.Any<GetFilteredStaffSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<StaffEntity>());

        var act = () => _staffService.DownloadStaffCsvAsync(1, new GetFilteredStaffCsvRequest(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal(StaffDownloadErrors.EmptyFilteredList, exception.Message);
    }

    [Fact]
    public async Task DownloadStaffCsvAsync_ShouldThrowBadRequest_WhenStaffDataIsIncomplete()
    {
        _staffRepository.GetStaffAccounts(Arg.Any<GetFilteredStaffSpecification>(), Arg.Any<CancellationToken>())
            .Returns([CreateStaff(role: null)]);

        var act = () => _staffService.DownloadStaffCsvAsync(1, new GetFilteredStaffCsvRequest(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal(StaffDownloadErrors.PartialDataUnavailable, exception.Message);
    }

    [Fact]
    public async Task DownloadStaffCsvAsync_ShouldThrowBadRequest_WhenCsvGenerationFails()
    {
        var staff = new[] { CreateStaff() };
        _staffRepository.GetStaffAccounts(Arg.Any<GetFilteredStaffSpecification>(), Arg.Any<CancellationToken>())
            .Returns(staff);
        _csvFileService.GetEntitiesCsvData<StaffCsvModel, StaffEntity>(Arg.Any<IEnumerable<StaffEntity>>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("CSV write failed"));

        var act = () => _staffService.DownloadStaffCsvAsync(1, new GetFilteredStaffCsvRequest(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal(StaffDownloadErrors.CsvGenerationFailed, exception.Message);
    }

    [Fact]
    public async Task DownloadStaffCsvAsync_ShouldReturnCsvBytes_WhenDataIsValid()
    {
        var expected = new byte[] { 1, 2, 3 };
        var staff = new[] { CreateStaff() };

        _staffRepository.GetStaffAccounts(Arg.Any<GetFilteredStaffSpecification>(), Arg.Any<CancellationToken>())
            .Returns(staff);
        _csvFileService.GetEntitiesCsvData<StaffCsvModel, StaffEntity>(Arg.Any<IEnumerable<StaffEntity>>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _staffService.DownloadStaffCsvAsync(1, new GetFilteredStaffCsvRequest(), CancellationToken.None);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task DownloadStaffListQueryHandler_ShouldDelegateToStaffService()
    {
        var staffService = Substitute.For<IStaffService>();
        var expected = new byte[] { 7, 8, 9 };
        var request = new GetFilteredStaffCsvRequest { FirstName = "Ann" };

        staffService.DownloadStaffCsvAsync(10, request, Arg.Any<CancellationToken>())
            .Returns(expected);

        var handler = new DownloadStaffListQueryHandler(staffService);

        var result = await handler.Handle(new DownloadStaffListQuery(10, request), CancellationToken.None);

        Assert.Equal(expected, result);
        await staffService.Received(1).DownloadStaffCsvAsync(10, request, Arg.Any<CancellationToken>());
    }

    private static StaffEntity CreateStaff(RoleEntity? role = null)
    {
        return new StaffEntity
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PasswordHash = "hash",
            ShopId = 1,
            Status = StaffStatusEnum.CreatePassword,
            Role = role ?? new RoleEntity { Id = 1, Name = "Admin" }
        };
    }
}
