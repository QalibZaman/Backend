using NettoRetail.Application.Models.Request.Staff;

namespace NettoRetail.Application.Services.IStaffCsvExportService;

public interface IStaffCsvExportService
{
    Task<byte[]> DownloadStaffCsvAsync(
        int userShopId,
        GetFilteredStaffCsvRequest request,
        CancellationToken token = default);
}
