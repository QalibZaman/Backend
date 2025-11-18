using NettoRetail.Application.Models.CsvFileModel;

namespace NettoRetail.Application.Services.IStaffService;

public interface IStaffServiсe
{
    Task CreateStaffAsync(
        StaffCsvModel staffCsvModel,
        int shopId,
        CancellationToken token = default);
}
