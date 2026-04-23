using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.IStaffCsvExportService;

public class StaffCsvExportService(
    IStaffRepository staffRepository,
    ICsvFileService csvFileService,
    ILogger<StaffCsvExportService> logger) : IStaffCsvExportService
{
    public async Task<byte[]> DownloadStaffCsvAsync(
        int userShopId,
        GetFilteredStaffCsvRequest request,
        CancellationToken token = default)
    {
        var specification = BuildSpecification(userShopId, request);
        var staff = await GetStaffForExportAsync(userShopId, specification, token);

        if (staff.Count == 0)
            throw new BadRequestException(StaffDownloadErrors.EmptyFilteredList);

        if (HasIncompleteStaffData(staff))
        {
            logger.LogWarning("Incomplete staff data detected during CSV export. ShopId: {ShopId}", userShopId);
            throw new BadRequestException(StaffDownloadErrors.PartialDataUnavailable);
        }

        try
        {
            return await csvFileService.GetEntitiesCsvData<StaffCsvModel, StaffEntity>(staff, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate staff CSV file. ShopId: {ShopId}", userShopId);
            throw new BadRequestException(StaffDownloadErrors.CsvGenerationFailed);
        }
    }

    private async Task<List<StaffEntity>> GetStaffForExportAsync(
        int userShopId,
        GetFilteredStaffSpecification specification,
        CancellationToken token)
    {
        try
        {
            return (await staffRepository.GetStaffAccounts(specification, token)).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve staff data for CSV export. ShopId: {ShopId}", userShopId);
            throw new BadRequestException(StaffDownloadErrors.PartialDataUnavailable);
        }
    }

    private static GetFilteredStaffSpecification BuildSpecification(int userShopId, GetFilteredStaffCsvRequest request)
    {
        var staffStatus = ParseStaffStatus(request.Status);

        return new GetFilteredStaffSpecification(
            shopId: userShopId,
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email,
            roleId: request.RoleId,
            status: staffStatus);
    }

    private static StaffStatusEnum? ParseStaffStatus(string? statusString)
    {
        if (string.IsNullOrWhiteSpace(statusString))
            return null;

        return Enum.TryParse<StaffStatusEnum>(statusString.Replace(" ", ""), true, out var status)
            ? status
            : null;
    }

    private static bool HasIncompleteStaffData(IEnumerable<StaffEntity> staff)
    {
        return staff.Any(x =>
            string.IsNullOrWhiteSpace(x.FirstName) ||
            string.IsNullOrWhiteSpace(x.LastName) ||
            string.IsNullOrWhiteSpace(x.Email) ||
            x.Role == null ||
            string.IsNullOrWhiteSpace(x.Role.Name));
    }
}
