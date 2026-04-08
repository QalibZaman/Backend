using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Services.IStaffService;

public interface IStaffService
{
    Task CreateStaffAsync(
        StaffCsvModel staffCsvModel,
        int shopId,
        CancellationToken token = default);

    Task<StaffEntity> CreateStaffAsync(
        CreateStaffRequest request,
        int shopId,
        CancellationToken token = default);

    Task CreateStaffAsync(
        IReadOnlyCollection<StaffCsvModel> staffCsvModels,
        int shopId,
        CancellationToken token = default);

    Task UpdateStaffAsync(
        UpdateStaffRequest request,
        StaffEntity staff,
        CancellationToken token = default);

    public StaffStatusEnum? ParseStaffStatus(string? statusString);
}
