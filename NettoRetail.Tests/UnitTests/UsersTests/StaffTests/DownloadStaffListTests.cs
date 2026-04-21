using MapsterMapper;
using Microsoft.Extensions.Logging;
using NettoRetail.Application.CQRS.Users.Queries.Staff;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Services.IStaffCsvExportService;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Tests.Helpers;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.UsersTests.StaffTests;

public class DownloadStaffListTests
{
    private readonly IStaffRepository _staffRepository;
    private readonly ICsvFileService _csvFileService;
    private readonly ILogger<StaffCsvExportService> _logger;
    private readonly StaffCsvExportService _staffCsvExportService;

    public DownloadStaffListTests()
    {
        _staffRepository = Substitute.For<IStaffRepository>();
        _csvFileService = Substitute.For<ICsvFileService>();
        _logger = Substitute.For<ILogger<StaffCsvExportService>>();

        _staffCsvExportService = new StaffCsvExportService(
            _staffRepository,
            _csvFileService,
            _logger);
    }

    [Fact]
    public async Task DownloadStaffCsvAsync_ShouldThrowBadRequest_WhenFilteredListIsEmpty()
    {
        _staffRepository.GetStaffAccounts(Arg.Any<GetFilteredStaffSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<StaffEntity>());

        var act = () => _staffCsvExportService.DownloadStaffCsvAsync(1, new GetFilteredStaffCsvRequest(), CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BadRequestException>(act);
        Assert.Equal(StaffDownloadErrors.EmptyFilteredList, exception.Message);
    }

    [Fact]
    public async Task DownloadStaffCsvAsync_ShouldThrowBadRequest_WhenStaffDataIsIncomplete()
    {
        _staffRepository.GetStaffAccounts(Arg.Any<GetFilteredStaffSpecification>(), Arg.Any<CancellationToken>())
            .Returns([CreateStaff(new RoleEntity { Id = 1, Name = string.Empty })]);

        var act = () => _staffCsvExportService.DownloadStaffCsvAsync(1, new GetFilteredStaffCsvRequest(), CancellationToken.None);

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
            .Returns(Task.FromException<byte[]>(new InvalidOperationException("CSV write failed")));

        var act = () => _staffCsvExportService.DownloadStaffCsvAsync(1, new GetFilteredStaffCsvRequest(), CancellationToken.None);

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

        var result = await _staffCsvExportService.DownloadStaffCsvAsync(1, new GetFilteredStaffCsvRequest(), CancellationToken.None);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task DownloadStaffListQueryHandler_ShouldDelegateToStaffService()
    {
        var staffCsvExportService = Substitute.For<IStaffCsvExportService>();
        var expected = new byte[] { 7, 8, 9 };
        var request = new GetFilteredStaffCsvRequest { FirstName = "Ann" };

        staffCsvExportService.DownloadStaffCsvAsync(10, request, Arg.Any<CancellationToken>())
            .Returns(expected);

        var handler = new DownloadStaffListQueryHandler(staffCsvExportService);

        var result = await handler.Handle(new DownloadStaffListQuery(10, request), CancellationToken.None);

        Assert.Equal(expected, result);
        await staffCsvExportService.Received(1).DownloadStaffCsvAsync(10, request, Arg.Any<CancellationToken>());
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
