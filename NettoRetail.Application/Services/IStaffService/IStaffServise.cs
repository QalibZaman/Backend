using NettoRetail.Application.Models.CsvFileModel;

namespace NettoRetail.Application.Services.IStaffService;

public interface IStaffServise
{
    Task CreateStaffAsync(
        StaffCsvModel staffCsvModel,
        int shopId,
        CancellationToken token = default);
}
