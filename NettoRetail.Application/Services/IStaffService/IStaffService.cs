using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.IStaffService;

public interface IStaffService
{
    Task CreateStaffAsync(
        StaffCsvModel staffCsvModel,
        int shopId,
        CancellationToken token = default);

    Task UpdateStaffAsync(
        UpdateStaffRequest request,
        StaffEntity staff,
        CancellationToken token = default);
}
